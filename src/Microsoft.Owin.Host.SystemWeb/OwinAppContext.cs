// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Routing;
using Microsoft.Owin.Builder;
using Microsoft.Owin.Host.SystemWeb.CallEnvironment;
using Microsoft.Owin.Host.SystemWeb.DataProtection;
using Microsoft.Owin.Host.SystemWeb.Infrastructure;
using Microsoft.Owin.Host.SystemWeb.WebSockets;
using Microsoft.Owin.Logging;
using Owin;

namespace Microsoft.Owin.Host.SystemWeb
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    internal partial class OwinAppContext
    {
        private const string TraceName = "Microsoft.Owin.Host.SystemWeb.OwinAppContext";

        private readonly ITrace _trace;

        private bool _detectWebSocketSupportStageTwoExecuted;
        private object _detectWebSocketSupportStageTwoLock;

        public OwinAppContext()
        {
            _trace = TraceFactory.Create(TraceName);
            AppName = HostingEnvironment.SiteName + HostingEnvironment.ApplicationID;
            if (string.IsNullOrWhiteSpace(AppName))
            {
                AppName = Guid.NewGuid().ToString();
            }
        }

        internal IDictionary<string, object> Capabilities { get; private set; }
        internal bool WebSocketSupport { get; set; }
        internal AppFunc AppFunc { get; set; }
        internal string AppName { get; private set; }

        internal void Initialize(Action<IAppBuilder> startup)
        {
            Capabilities = new ConcurrentDictionary<string, object>();

            var builder = new AppBuilder();
            builder.Properties[Constants.OwinVersionKey] = Constants.OwinVersion;
            builder.Properties[Constants.HostTraceOutputKey] = TraceTextWriter.Instance;
            builder.Properties[Constants.HostAppNameKey] = AppName;
            builder.Properties[Constants.HostOnAppDisposingKey] = OwinApplication.ShutdownToken;
            builder.Properties[Constants.HostReferencedAssemblies] = new ReferencedAssembliesWrapper();
            builder.Properties[Constants.ServerCapabilitiesKey] = Capabilities;
            builder.Properties[Constants.SecurityDataProtectionProvider] = new MachineKeyDataProtectionProvider().ToOwinFunction();
            builder.SetLoggerFactory(new DiagnosticsLoggerFactory());

            Capabilities[Constants.SendFileVersionKey] = Constants.SendFileVersion;

            CompilationSection compilationSection = (CompilationSection)System.Configuration.ConfigurationManager.GetSection(@"system.web/compilation");
            bool isDebugEnabled = compilationSection.Debug;
            if (isDebugEnabled)
            {
                builder.Properties[Constants.HostAppModeKey] = Constants.AppModeDevelopment;
            }

            DetectWebSocketSupportStageOne();

            try
            {
                startup(builder);
            }
            catch (Exception ex)
            {
                _trace.WriteError(Resources.Trace_EntryPointException, ex);
                throw;
            }

            AppFunc = (AppFunc)builder.Build(typeof(AppFunc));
        }

        public OwinCallContext CreateCallContext(
            RequestContext requestContext,
            string requestPathBase,
            string requestPath,
            AsyncCallback callback,
            object extraData)
        {
            DetectWebSocketSupportStageTwo(requestContext);
            return new OwinCallContext(this, requestContext, requestPathBase, requestPath, callback, extraData);
        }

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
