// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if !NET40

using System.Diagnostics;
using System.Threading;
using System.Web;
using System.Web.Routing;
using Microsoft.Owin.Host.SystemWeb.WebSockets;

namespace Microsoft.Owin.Host.SystemWeb
{
    internal partial class OwinAppContext
    {
        private bool _detectWebSocketSupportStageTwoExecuted;
        private object _detectWebSocketSupportStageTwoLock;

        private void DetectWebSocketSupportStageOne()
        {
            // There is no explicit API to detect server side websockets, just check for v4.5 / Win8.
            // Per request we can provide actual verification.
            if (HttpRuntime.IISVersion != null && HttpRuntime.IISVersion.Major >= 8)
            {
                WebSocketSupport = true;
                Capabilities[Constants.WebSocketVersionKey] = Constants.WebSocketVersion;
            }
            else
            {
                _trace.Write(TraceEventType.Information, Resources.Trace_WebSocketsSupportNotDetected);
            }
        }

        private void DetectWebSocketSupportStageTwo(RequestContext requestContext)
        {
            object ignored = null;
            if (WebSocketSupport)
            {
                LazyInitializer.EnsureInitialized(
                    ref ignored,
                    ref _detectWebSocketSupportStageTwoExecuted,
                    ref _detectWebSocketSupportStageTwoLock,
                    () =>
                    {
                        string webSocketVersion = requestContext.HttpContext.Request.ServerVariables[WebSocketConstants.AspNetServerVariableWebSocketVersion];
                        if (string.IsNullOrEmpty(webSocketVersion))
                        {
                            Capabilities.Remove(Constants.WebSocketVersionKey);
                            WebSocketSupport = false;
                            _trace.Write(TraceEventType.Information, Resources.Trace_WebSocketsSupportNotDetected);
                        }
                        else
                        {
                            _trace.Write(TraceEventType.Information, Resources.Trace_WebSocketsSupportDetected);
                        }
                        return null;
                    });
            }
        }
    }
}

#else

using ResharperCodeFormattingWorkaround = System.Object;

#endif
