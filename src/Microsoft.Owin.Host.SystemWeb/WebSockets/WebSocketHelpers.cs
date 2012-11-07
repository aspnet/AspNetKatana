// <copyright file="WebSocketHelpers.cs" company="Katana contributors">
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

#if NET40
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Owin.Host.SystemWeb.CallEnvironment;

#else
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web;
using System.Web.WebSockets;
using Microsoft.Owin.Host.SystemWeb.CallEnvironment;

#endif

namespace Microsoft.Owin.Host.SystemWeb.WebSockets
{
    using WebSocketFunc =
        Func<IDictionary<string, object>, // WebSocket environment
            Task /* Complete */>;

    // Provides WebSocket support on .NET 4.5+.  Note that #if !NET40 is only used for items that cannot compile on NET40.
    internal static class WebSocketHelpers
    {
        private static bool? _serverHasWebSocketsEnabled;

        internal static bool CheckIfServerHasWebSocketsEnabled(HttpContextBase context)
        {
            if (!_serverHasWebSocketsEnabled.HasValue)
            {
                _serverHasWebSocketsEnabled =
                    !String.IsNullOrEmpty(context.Request.ServerVariables[WebSocketConstants.AspNetServerVariableWebSocketVersion]);
            }
            return _serverHasWebSocketsEnabled.Value;
        }

        internal static bool IsAspNetWebSocketRequest(HttpContextBase context)
        {
            bool isWebSocketRequest = false;
            if (CheckIfServerHasWebSocketsEnabled(context))
            {
#if !NET40
                if (context.IsWebSocketRequest)
                {
                    isWebSocketRequest = true;
                }
#endif
            }
            return isWebSocketRequest;
        }

        internal static string GetWebSocketSubProtocol(AspNetDictionary env, IDictionary<string, object> accpetOptions)
        {
            IDictionary<string, string[]> reponseHeaders = env.ResponseHeaders;

            // Remove the subprotocol header, Accept will re-add it.
            string subProtocol = null;
            string[] subProtocols;
            if (reponseHeaders.TryGetValue(WebSocketConstants.SecWebSocketProtocol, out subProtocols) && subProtocols.Length > 0)
            {
                subProtocol = subProtocols[0];
                reponseHeaders.Remove(WebSocketConstants.SecWebSocketProtocol);
            }

            if (accpetOptions != null && accpetOptions.ContainsKey(WebSocketConstants.WebSocketSubProtocolKey))
            {
                subProtocol = accpetOptions.Get<string>(WebSocketConstants.WebSocketSubProtocolKey);
            }

            return subProtocol;
        }

#if !NET40
        internal static void DoWebSocketUpgrade(HttpContextBase context, AspNetDictionary env, WebSocketFunc webSocketFunc,
            IDictionary<string, object> acceptOptions)
        {
            var options = new AspNetWebSocketOptions();
            options.SubProtocol = WebSocketHelpers.GetWebSocketSubProtocol(env, acceptOptions);

            context.AcceptWebSocketRequest(async webSocketContext =>
            {
                OwinWebSocketWrapper wrapper = null;
                try
                {
                    wrapper = new OwinWebSocketWrapper(webSocketContext);
                    await webSocketFunc(wrapper.Environment);
                    await wrapper.CleanupAsync();
                    wrapper.Dispose();
                }
                catch (Exception ex)
                {
                    if (wrapper != null)
                    {
                        wrapper.Dispose();
                    }
                    Trace.WriteLine(Resources.Exception_ProcessingWebSocket);
                    Trace.WriteLine(ex.ToString());
                    throw;
                }
            }, options);
        }
#endif
    }
}
