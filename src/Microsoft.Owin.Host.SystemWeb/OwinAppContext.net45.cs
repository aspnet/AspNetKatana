// <copyright file="OwinAppContext.cs" company="Katana contributors">
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

using System.Diagnostics;
using System.Threading;
using System.Web;
using System.Web.Routing;
using Microsoft.Owin.Host.SystemWeb.Infrastructure;
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
                        var webSocketVersion = requestContext.HttpContext.Request.ServerVariables[WebSocketConstants.AspNetServerVariableWebSocketVersion];
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
