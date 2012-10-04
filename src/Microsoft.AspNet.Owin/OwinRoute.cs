//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Katana Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;

namespace Microsoft.AspNet.Owin
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