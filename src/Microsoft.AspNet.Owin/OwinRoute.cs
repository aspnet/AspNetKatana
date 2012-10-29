// <copyright file="OwinRoute.cs" company="Katana contributors">
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
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;

namespace Microsoft.Owin.Host.SystemWeb
{
    using AppDelegate = Func<IDictionary<string, object>, Task>;

    public class OwinRoute : RouteBase
    {
        private readonly string _pathBase;
        private readonly Func<AppDelegate> _appAccessor;

        public OwinRoute(string pathBase, Func<AppDelegate> appAccessor)
        {
            _pathBase = Utils.NormalizePath(HttpRuntime.AppDomainAppVirtualPath) + Utils.NormalizePath(pathBase);
            _appAccessor = appAccessor;
        }

        public override RouteData GetRouteData(HttpContextBase httpContext)
        {
            var requestPath = httpContext.Request.CurrentExecutionFilePath + httpContext.Request.PathInfo;

            var startsWithPathBase = requestPath.StartsWith(_pathBase, StringComparison.OrdinalIgnoreCase);
            return startsWithPathBase ? new RouteData(this, new OwinRouteHandler(_pathBase, requestPath.Substring(_pathBase.Length), _appAccessor)) : null;
        }

        public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
        {
            return null;
        }
    }
}
