// <copyright file="OwinCallContext.WebSockets.net45.cs" company="Katana contributors">
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

#if !NET40

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web.WebSockets;
using Microsoft.Owin.Host.SystemWeb.CallEnvironment;
using Microsoft.Owin.Host.SystemWeb.WebSockets;

namespace Microsoft.Owin.Host.SystemWeb
{
    using WebSocketAccept =
        Action<IDictionary<string, object>, // WebSocket Accept parameters
            Func<IDictionary<string, object>, // WebSocket environment
                Task /* Complete */>>;
    using WebSocketFunc =
        Func<IDictionary<string, object>, // WebSocket environment
            Task /* Complete */>;

    internal partial class OwinCallContext
    {
        private WebSocketFunc _webSocketFunc;

        bool AspNetDictionary.IPropertySource.TryGetWebSocketAccept(ref WebSocketAccept value)
        {
            if (_appContext.WebSocketSupport && _httpContext.IsWebSocketRequest)
            {
                value = new WebSocketAccept(DoWebSocketUpgrade);
                return true;
            }
            return false;
        }

        private void DoWebSocketUpgrade(IDictionary<string, object> acceptOptions, WebSocketFunc webSocketFunc)
        {
            _env.ResponseStatusCode = 101;
            _webSocketFunc = webSocketFunc;

            var options = new AspNetWebSocketOptions();
            options.SubProtocol = GetWebSocketSubProtocol(_env, acceptOptions);

            OnStart();
            _httpContext.AcceptWebSocketRequest(AcceptCallback, options);
        }

        private async Task AcceptCallback(AspNetWebSocketContext webSocketContext)
        {
            OwinWebSocketWrapper wrapper = null;
            try
            {
                wrapper = new OwinWebSocketWrapper(webSocketContext);
                await _webSocketFunc(wrapper.Environment);
                await wrapper.CleanupAsync();
                wrapper.Dispose();
            }
            catch (Exception ex)
            {
                if (wrapper != null)
                {
                    wrapper.Cancel();
                    wrapper.Dispose();
                }
                Trace.WriteLine(Resources.Exception_ProcessingWebSocket);
                Trace.WriteLine(ex.ToString());
                throw;
            }
        }

        private static string GetWebSocketSubProtocol(AspNetDictionary env, IDictionary<string, object> accpetOptions)
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
    }
}

#else

using ResharperCodeFormattingWorkaround = System.Object;

#endif
