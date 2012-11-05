// <copyright file="OwinBuilder.cs" company="Katana contributors">
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
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using Microsoft.Owin.Host.SystemWeb.CallEnvironment;
using Owin;
using Owin.Builder;
using Owin.Loader;

namespace Microsoft.Owin.Host.SystemWeb
{
    internal static class OwinBuilder
    {
        internal static readonly Func<IDictionary<string, object>, Task> NotFound = env =>
        {
            env[Constants.OwinResponseStatusCodeKey] = 404;
            return TaskHelpers.Completed();
        };

        internal static Func<IDictionary<string, object>, Task> Build()
        {
            string configuration = ConfigurationManager.AppSettings[Constants.OwinConfiguration];
            var loader = new DefaultLoader();
            Action<IAppBuilder> startup = loader.Load(configuration ?? string.Empty);
            return Build(startup);
        }

        internal static Func<IDictionary<string, object>, Task> Build(Action<IAppBuilder> startup)
        {
            if (startup == null)
            {
                return null;
            }

            var builder = new AppBuilder();
            builder.Properties[Constants.BuilderDefaultAppKey] = NotFound;
            builder.Properties[Constants.HostTraceOutputKey] = TraceTextWriter.Instance;
            builder.Properties[Constants.HostAppNameKey] = HostingEnvironment.SiteName;
            builder.Properties[Constants.HostOnAppDisposingKey] = OwinApplication.ShutdownToken;

            var capabilities = new Dictionary<string, object>();
            builder.Properties[Constants.ServerCapabilitiesKey] = capabilities;

            capabilities[Constants.ServerNameKey] = Constants.ServerName;
            capabilities[Constants.ServerVersionKey] = Constants.ServerVersion;
            capabilities[Constants.SendFileVersionKey] = Constants.SendFileVersion;

            builder.UseFunc(next => env =>
            {
                env[Constants.ServerCapabilitiesKey] = capabilities;
                env[Constants.HostOnAppDisposingKey] = OwinApplication.ShutdownToken;
                return next(env);
            });

#if !NET40
            DetectWebSocketSupport(builder);
#endif
            startup(builder);
            return builder.Build<Func<IDictionary<string, object>, Task>>();
        }

#if !NET40
        private static void DetectWebSocketSupport(IAppBuilder builder)
        {
            // There is no explicit API to detect server side websockets, just check for v4.5 / Win8.
            // Per request we can provide actual verification.
            if (HttpRuntime.IISVersion != null && HttpRuntime.IISVersion.Major >= 8)
            {
                var capabilities = builder.Properties.Get<IDictionary<string, object>>(Constants.ServerCapabilitiesKey);
                capabilities[Constants.WebSocketVersionKey] = Constants.WebSocketVersion;
                Trace.WriteLine(Resources.WebSockets_SupportDetected);
            }
            else
            {
                Trace.WriteLine(Resources.WebSockets_SupportNotDetected);
            }
        }
#endif
    }
}
