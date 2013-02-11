// <copyright file="OwinHttpModule.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
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
// </copyright>

using System;
using System.Configuration;
using System.Diagnostics;
using System.Web;
using Microsoft.Owin.Host.SystemWeb.Infrastructure;

namespace Microsoft.Owin.Host.SystemWeb
{
    internal sealed class OwinHttpModule : IHttpModule
    {
        private const string TraceName = "Microsoft.Owin.Host.SystemWeb.OwinHttpModule";

        private readonly ITrace _trace;

        internal OwinHttpModule()
        {
            _trace = TraceFactory.Create(TraceName);
        }

        public void Init(HttpApplication context)
        {
            if (OwinApplication.Instance == null)
            {
                _trace.Write(TraceEventType.Warning, Resources.Trace_NoOwinEntryPointFound);
                return;
            }

            string handleAllRequests = ConfigurationManager.AppSettings[Constants.OwinHandleAllRequests];

            if (string.Equals("true", handleAllRequests, StringComparison.OrdinalIgnoreCase))
            {
                var handler = new OwinHttpHandler(
                    pathBase: Utils.NormalizePath(HttpRuntime.AppDomainAppVirtualPath),
                    appAccessor: OwinApplication.Accessor);

                context.PostResolveRequestCache += (sender, e) => context.Context.RemapHandler(handler);
            }
        }

        public void Dispose()
        {
        }
    }
}
