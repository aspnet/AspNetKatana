// <copyright file="OwinHttpModule.cs" company="Katana contributors">
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
using System.Configuration;
using System.Web;

namespace Microsoft.Owin.Host.SystemWeb
{
    public class OwinHttpModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            try
            {
                if (OwinApplication.Instance == null)
                {
                    return;
                }
            }
            catch
            {
                // TODO: what is the best way to handle initialization errors? or apps w/out startup class?
                return;
            }

            var handleAllRequests = ConfigurationManager.AppSettings["owin:HandleAllRequests"];

            if (string.Equals("True", handleAllRequests, StringComparison.InvariantCultureIgnoreCase))
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
