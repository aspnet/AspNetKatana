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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Routing;
using Microsoft.Owin.Host.SystemWeb.CallEnvironment;
using Microsoft.Owin.Host.SystemWeb.Infrastructure;
using Owin;
using Owin.Builder;

namespace Microsoft.Owin.Host.SystemWeb
{
    internal partial class OwinAppContext
    {
        private const string TraceName = "Microsoft.Owin.Host.SystemWeb.OwinAppContext";

        internal static readonly Func<IDictionary<string, object>, Task> NotFound = env =>
        {
            env[Constants.OwinResponseStatusCodeKey] = 404;
            return TaskHelpers.Completed();
        };

        private readonly ITrace _trace;

        public OwinAppContext()
        {
            _trace = TraceFactory.Create(TraceName);
        }

        internal IDictionary<string, object> Capabilities { get; private set; }
        internal bool WebSocketSupport { get; set; }
        internal Func<IDictionary<string, object>, Task> AppFunc { get; set; }

        internal void Initialize(Action<IAppBuilder> startup)
        {
            Capabilities = new ConcurrentDictionary<string, object>();

            var builder = new AppBuilder();
            builder.Properties[Constants.BuilderDefaultAppKey] = NotFound;
            builder.Properties[Constants.HostTraceOutputKey] = TraceTextWriter.Instance;
            builder.Properties[Constants.HostAppNameKey] = HostingEnvironment.SiteName;
            builder.Properties[Constants.HostOnAppDisposingKey] = OwinApplication.ShutdownToken;
            builder.Properties[Constants.ServerCapabilitiesKey] = Capabilities;

            Capabilities[Constants.ServerNameKey] = Constants.ServerName;
            Capabilities[Constants.ServerVersionKey] = Constants.ServerVersion;
            Capabilities[Constants.SendFileVersionKey] = Constants.SendFileVersion;

#if !NET40
            DetectWebSocketSupportStageOne();
#endif
            startup(builder);

            AppFunc = builder.Build<Func<IDictionary<string, object>, Task>>();
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
