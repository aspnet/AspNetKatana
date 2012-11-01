// -----------------------------------------------------------------------
// <copyright file="WebSocketHelpers.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
#if !NET40
using System.Web.WebSockets;
#endif
using Microsoft.Owin.Host.SystemWeb.CallEnvironment;

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

        internal static void DoWebSocketUpgrade(HttpContextBase context, AspNetDictionary env, WebSocketFunc webSocketFunc, 
            IDictionary<string, object> acceptOptions)
        {
#if !NET40
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
                catch (Exception)
                {
                    if (wrapper != null)
                    {
                        wrapper.Dispose();
                    }
                    // TODO: Log
                    throw;
                }
            }, options);
#endif
        }
    }
}
