// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.WebSockets;
using Microsoft.Owin.Host.SystemWeb.CallEnvironment;
using Microsoft.Owin.Host.SystemWeb.Infrastructure;
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
            if (webSocketFunc == null)
            {
                throw new ArgumentNullException("webSocketFunc");
            }

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
                // Making sure to close the web socket is not necessary, Asp.Net will do this for us. 
                wrapper.Dispose();
            }
            catch (Exception ex)
            {
                if (wrapper != null)
                {
                    wrapper.Cancel();
                    wrapper.Dispose();
                }

                // traced as warning here because it is re-thrown
                Trace.WriteWarning(Resources.Trace_WebSocketException, ex);
                throw;
            }
        }

        private static string GetWebSocketSubProtocol(AspNetDictionary env, IDictionary<string, object> accpetOptions)
        {
            IDictionary<string, string[]> reponseHeaders = env.ResponseHeaders;

            // Remove the sub-protocol header, Accept will re-add it.
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
