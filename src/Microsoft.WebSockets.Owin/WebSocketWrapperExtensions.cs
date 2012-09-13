using Microsoft.WebSockets.Owin;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Net.WebSockets;
using System.Web.WebSockets;

namespace Owin
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    using WebSocketFunc =
        Func
        <
            IDictionary<string, object>, // WebSocket environment
            Task // Complete
        >;

    using WebSocketReceiveTuple = Tuple
        <
            int /* messageType */,
            bool /* endOfMessage */,
            int? /* count */,
            int? /* closeStatus */,
            string /* closeStatusDescription */
        >;

    // This class is a 4.5 dependent middleware that HttpListener or AspNet can load at startup if they detect they are running on .NET 4.5.
    // This permits those server wrappers to remain as 4.0 components while still providing 4.5 functionality.
    public static class WebSocketWrapperExtensions
    {
        public static IAppBuilder UseWebSocketWrapper(this IAppBuilder builder)
        {
            if (builder.Properties.ContainsKey("aspnet.Version"))
            {
                return UseAspNetWebSocketWrapper(builder);
            }
            if (builder.Properties.ContainsKey("httplistener.Version"))
            {
                return UseHttpListenerWebSocketWrapper(builder);
            }
            return builder;
        }

        public static IAppBuilder UseAspNetWebSocketWrapper(this IAppBuilder builder)
        {
            if (!builder.Properties.ContainsKey(Constants.WebSocketSupportKey) &&
                HttpRuntime.IISVersion != null &&
                HttpRuntime.IISVersion.Major >= 8)
            {
                builder.Properties[Constants.WebSocketSupportKey] = Constants.WebSocketSupport;
                return builder.UseFunc(AspNetMiddleware);
            }
            return builder;
        }

        public static IAppBuilder UseHttpListenerWebSocketWrapper(this IAppBuilder builder)
        {
            if (!builder.Properties.ContainsKey(Constants.WebSocketSupportKey))
            {
                builder.Properties[Constants.WebSocketSupportKey] = Constants.WebSocketSupport;
                return builder.UseFunc(HttpListenerMiddleware);
            }
            return builder;
        }

        private const string AspNetServerVariableWebSocketVersion = "WEBSOCKET_VERSION";

        public static AppFunc AspNetMiddleware(AppFunc app)
        {
            return async env =>
            {
                var context = env.Get<HttpContextBase>("System.Web.HttpContextBase");

                if (String.IsNullOrEmpty(context.Request.ServerVariables[AspNetServerVariableWebSocketVersion]))
                {
                    // not supported after all - do nothing else and pass through
                    await app(env);
                    return;
                }

                env[Constants.WebSocketSupportKey] = Constants.WebSocketSupport;

                bool isWebSocketRequest = false;
                if (context != null)
                {
                    // Not implemented by custom contexts or FakeN.Web.
                    try
                    {
                        if (context.IsWebSocketRequest)
                        {
                            isWebSocketRequest |= true;
                        }
                    }
                    catch (NotImplementedException) { }
                }

                if (isWebSocketRequest)
                {
                    IDictionary<string, string[]> reponseHeaders = env.Get<IDictionary<string, string[]>>(Constants.ResponseHeadersKey);
                    env[Constants.WebSocketSupportKey] = Constants.WebSocketSupport;

                    await app(env);

                    WebSocketFunc webSocketFunc = env.Get<WebSocketFunc>(Constants.WebSocketFuncKey);
                    int statusCode = env.Get<int>(Constants.ResponseStatusCodeKey);

                    // If the app requests a websocket upgrade, provide a fake body delegate to do so.
                    if (statusCode == 101 && webSocketFunc != null)
                    {
                        string subProtocol = null;
                        string[] subProtocols;
                        if (reponseHeaders.TryGetValue(Constants.SecWebSocketProtocol, out subProtocols) && subProtocols.Length > 0)
                        {
                            subProtocol = subProtocols[0];
                            reponseHeaders.Remove(Constants.SecWebSocketProtocol);
                        }

                        AspNetWebSocketOptions options = new AspNetWebSocketOptions();
                        options.SubProtocol = subProtocol;

                        context.AcceptWebSocketRequest(async webSocketContext =>
                        {
                            try
                            {
                                OwinWebSocketWrapper wrapper = new OwinWebSocketWrapper(webSocketContext);
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
                HttpListenerContext context = env.Get<HttpListenerContext>("System.Net.HttpListenerContext");

                env[Constants.WebSocketSupportKey] = Constants.WebSocketSupport;

                if (context != null && context.Request.IsWebSocketRequest)
                {
                    IDictionary<string, string[]> reponseHeaders = env.Get<IDictionary<string, string[]>>(Constants.ResponseHeadersKey);
                    env[Constants.WebSocketSupportKey] = Constants.WebSocketSupport;

                    await app(env);

                    WebSocketFunc webSocketFunc = env.Get<WebSocketFunc>(Constants.WebSocketFuncKey);
                    int statusCode = env.Get<int>(Constants.ResponseStatusCodeKey);

                    // TODO: This can only trigger if the response stream wasn't written to.

                    // If the app requests a websocket upgrade, provide a fake body delegate to do so.
                    if (statusCode == 101 && webSocketFunc != null)
                    {
                        string subProtocol = null;
                        string[] subProtocols;
                        if (reponseHeaders.TryGetValue(Constants.SecWebSocketProtocol, out subProtocols) && subProtocols.Length > 0)
                        {
                            subProtocol = subProtocols[0];
                            reponseHeaders.Remove(Constants.SecWebSocketProtocol);
                        }

                        // TODO: Other parameters?
                        WebSocketContext webSocketContext = await context.AcceptWebSocketAsync(subProtocol);
                        OwinWebSocketWrapper wrapper = new OwinWebSocketWrapper(webSocketContext);
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
    }
}
