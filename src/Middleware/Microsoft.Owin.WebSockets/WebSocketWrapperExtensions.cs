// <copyright file="WebSocketWrapperExtensions.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
// </copyright>
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.WebSockets;
using Microsoft.Owin.WebSockets;

namespace Owin
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    using WebSocketAccept =
        Action<IDictionary<string, object>, // WebSocket Accept parameters
            Func<IDictionary<string, object>, // WebSocket environment
                Task /* Complete */>>;
    using WebSocketFunc =
        Func<IDictionary<string, object>, // WebSocket environment
            Task /* Complete */>;

    // This class is a 4.5 dependent middleware that HttpListener or AspNet can load at startup if they detect they are running on .NET 4.5.
    // This permits those server wrappers to remain as 4.0 components while still providing 4.5 functionality.
    public static class WebSocketWrapperExtensions
    {
        private const string AspNetServerVariableWebSocketVersion = "WEBSOCKET_VERSION";

        public static IAppBuilder UseWebSocketWrapper(this IAppBuilder builder)
        {
            Version ver;
            if (TryGetVersion(builder, "msaspnet.AdapterVersion", out ver) && ver >= new Version(0, 7))
            {
                return UseAspNetWebSocketWrapper(builder);
            }
            if (TryGetVersion(builder, "mshttplistener.AdapterVersion", out ver) && ver >= new Version(0, 7))
            {
                return UseHttpListenerWebSocketWrapper(builder);
            }
            return builder;
        }

        private static bool TryGetVersion(IAppBuilder builder, string key, out Version version)
        {
            object value;
            version = null;

            var capabilities = builder.Properties.Get<IDictionary<string, object>>(Constants.ServerCapabilitiesKey);

            return capabilities != null && capabilities.TryGetValue(key, out value) &&
                Version.TryParse(Convert.ToString(value), out version);
        }

        public static IAppBuilder UseAspNetWebSocketWrapper(this IAppBuilder builder)
        {
            var capabilities = builder.Properties.Get<IDictionary<string, object>>(Constants.ServerCapabilitiesKey);
            if (string.IsNullOrWhiteSpace(capabilities.Get<string>(Constants.WebSocketVersionKey))
                && HttpRuntime.IISVersion != null
                && HttpRuntime.IISVersion.Major >= 8)
            {
                capabilities[Constants.WebSocketVersionKey] = Constants.WebSocketVersion;
                return builder.UseFunc(AspNetMiddleware);
            }
            return builder;
        }

        public static IAppBuilder UseHttpListenerWebSocketWrapper(this IAppBuilder builder)
        {
            var capabilities = builder.Properties.Get<IDictionary<string, object>>(Constants.ServerCapabilitiesKey);
            if (string.IsNullOrWhiteSpace(capabilities.Get<string>(Constants.WebSocketVersionKey)))
            {
                capabilities[Constants.WebSocketVersionKey] = Constants.WebSocketVersion;
                return builder.UseFunc(HttpListenerMiddleware);
            }
            return builder;
        }

        public static AppFunc AspNetMiddleware(AppFunc app)
        {
            return async env =>
            {
                var context = env.Get<HttpContextBase>(typeof(HttpContextBase).FullName);

                if (context != null && String.IsNullOrEmpty(context.Request.ServerVariables[AspNetServerVariableWebSocketVersion]))
                {
                    // var capabilities = env.Get< // TODO: Remove support from the capabilities list.  // TODO: There must be a more reliable way to detect this at startup.
                    // not supported after all - do nothing else and pass through
                    await app(env);
                    return;
                }

                if (context != null && IsAspNetWebSocketRequest(context))
                {
                    WebSocketFunc webSocketFunc = null;
                    IDictionary<string, object> acceptOptions = null;
                    env[Constants.WebSocketAcceptKey] = new WebSocketAccept(
                        (options, callback) =>
                        {
                            env[Constants.ResponseStatusCodeKey] = 101;
                            acceptOptions = options;
                            webSocketFunc = callback;
                        });

                    await app(env);

                    // If the app requests a websocket upgrade, provide a fake body delegate to do so.
                    if (webSocketFunc != null && env.Get<int>(Constants.ResponseStatusCodeKey) == 101)
                    {
                        var options = new AspNetWebSocketOptions();
                        options.SubProtocol = GetSubProtocol(env, acceptOptions);

                        context.AcceptWebSocketRequest(async webSocketContext =>
                        {
                            try
                            {
                                var wrapper = new OwinWebSocketWrapper(webSocketContext, env.Get<CancellationToken>(Constants.CallCancelledKey));
                                await webSocketFunc(wrapper.Environment);
                                await wrapper.CleanupAsync();
                            }
                            catch (Exception)
                            {
                                // TODO: Log
                                throw;
                            }
                        }, options);
                    }
                }
                else
                {
                    await app(env);
                }
            };
        }

        public static AppFunc HttpListenerMiddleware(AppFunc app)
        {
            return async env =>
            {
                HttpListenerContext context = env.Get<HttpListenerContext>(typeof(HttpListenerContext).FullName);

                if (context != null && context.Request.IsWebSocketRequest)
                {
                    Stream outputStream = env.Get<Stream>(Constants.ResponseBodyKey);

                    WebSocketFunc webSocketFunc = null;
                    IDictionary<string, object> acceptOptions = null;
                    env[Constants.WebSocketAcceptKey] = new WebSocketAccept(
                        (options, callback) =>
                        {
                            env[Constants.ResponseStatusCodeKey] = 101;
                            acceptOptions = options;
                            webSocketFunc = callback;
                        });

                    await app(env);

                    if (webSocketFunc != null && env.Get<int>(Constants.ResponseStatusCodeKey) == 101)
                    {
                        string subProtocol = GetSubProtocol(env, acceptOptions);

                        // Trigger the OnSendingHeaders event.
                        outputStream.Flush();

                        // TODO: Other parameters?
                        WebSocketContext webSocketContext = await context.AcceptWebSocketAsync(subProtocol);
                        OwinWebSocketWrapper wrapper = new OwinWebSocketWrapper(webSocketContext, env.Get<CancellationToken>(Constants.CallCancelledKey));
                        await webSocketFunc(wrapper.Environment);
                        await wrapper.CleanupAsync();
                    }
                }
                else
                {
                    await app(env);
                }
            };
        }

        private static bool IsAspNetWebSocketRequest(HttpContextBase context)
        {
            bool isWebSocketRequest = false;
            if (context != null)
            {
                // Not implemented by custom contexts or FakeN.Web.
                try
                {
                    if (context.IsWebSocketRequest)
                    {
                        isWebSocketRequest = true;
                    }
                }
                catch (NotImplementedException)
                {
                }
            }
            return isWebSocketRequest;
        }

        private static string GetSubProtocol(IDictionary<string, object> env, IDictionary<string, object> acceptOptions)
        {
            IDictionary<string, string[]> reponseHeaders = env.Get<IDictionary<string, string[]>>(Constants.ResponseHeadersKey);

            // Remove the subprotocol header, Accept will re-add it.
            string subProtocol = null;
            string[] subProtocols;
            if (reponseHeaders.TryGetValue(Constants.SecWebSocketProtocol, out subProtocols) && subProtocols.Length > 0)
            {
                subProtocol = subProtocols[0];
                reponseHeaders.Remove(Constants.SecWebSocketProtocol);
            }

            if (acceptOptions != null && acceptOptions.ContainsKey(Constants.WebSocketSubProtocolKey))
            {
                subProtocol = acceptOptions.Get<string>(Constants.WebSocketSubProtocolKey);
            }

            return subProtocol;
        }
    }
}
