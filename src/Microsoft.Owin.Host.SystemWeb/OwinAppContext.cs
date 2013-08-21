// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Routing;
using Microsoft.Owin.Builder;
using Microsoft.Owin.Host.SystemWeb.CallEnvironment;
using Microsoft.Owin.Host.SystemWeb.DataProtection;
using Microsoft.Owin.Host.SystemWeb.Infrastructure;
using Microsoft.Owin.Logging;
using Owin;

namespace Microsoft.Owin.Host.SystemWeb
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    internal partial class OwinAppContext
    {
        private const string TraceName = "Microsoft.Owin.Host.SystemWeb.OwinAppContext";

        private readonly ITrace _trace;

        public OwinAppContext()
        {
            _trace = TraceFactory.Create(TraceName);
        }

        internal IDictionary<string, object> Capabilities { get; private set; }
        internal bool WebSocketSupport { get; set; }
        internal AppFunc AppFunc { get; set; }

        internal void Initialize(Action<IAppBuilder> startup)
        {
            Capabilities = new ConcurrentDictionary<string, object>();

            var builder = new AppBuilder();
            builder.Properties[Constants.OwinVersionKey] = Constants.OwinVersion;
            builder.Properties[Constants.HostTraceOutputKey] = TraceTextWriter.Instance;
            builder.Properties[Constants.HostAppNameKey] = HostingEnvironment.SiteName;
            builder.Properties[Constants.HostOnAppDisposingKey] = OwinApplication.ShutdownToken;
            builder.Properties[Constants.HostReferencedAssemblies] = new ReferencedAssembliesWrapper();
            builder.Properties[Constants.ServerCapabilitiesKey] = Capabilities;
            builder.Properties[Constants.SecurityDataProtectionProvider] = new MachineKeyDataProtectionProvider().ToOwinFunction();
            builder.SetLoggerFactory(new DiagnosticsLoggerFactory());

            Capabilities[Constants.ServerNameKey] = Constants.ServerName;
            Capabilities[Constants.SendFileVersionKey] = Constants.SendFileVersion;

            CompilationSection compilationSection = (CompilationSection)System.Configuration.ConfigurationManager.GetSection(@"system.web/compilation");
            bool isDebugEnabled = compilationSection.Debug;
            if (isDebugEnabled)
            {
                builder.Properties[Constants.HostAppModeKey] = Constants.AppModeDevelopment;
            }

#if !NET40
            DetectWebSocketSupportStageOne();
#endif
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
#if !NET40
            DetectWebSocketSupportStageTwo(requestContext);
#endif
            return new OwinCallContext(this, requestContext, requestPathBase, requestPath, callback, extraData);
        }
    }
}
