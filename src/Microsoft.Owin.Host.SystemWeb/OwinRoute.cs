// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Web;
using System.Web.Routing;

namespace Microsoft.Owin.Host.SystemWeb
{
    internal sealed class OwinRoute : RouteBase
    {
        private readonly string _pathBase;
        private readonly Func<OwinAppContext> _appAccessor;

        internal OwinRoute(string pathBase, Func<OwinAppContext> appAccessor)
        {
            _pathBase = Utils.NormalizePath(HttpRuntime.AppDomainAppVirtualPath) + Utils.NormalizePath(pathBase);
            _appAccessor = appAccessor;
        }

        public override RouteData GetRouteData(HttpContextBase httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException("httpContext");
            }

            string requestPath = httpContext.Request.CurrentExecutionFilePath + httpContext.Request.PathInfo;

            int requestPathLength = requestPath.Length;
            int pathBaseLength = _pathBase.Length;
            if (requestPathLength < pathBaseLength)
            {
                return null;
            }
            if (requestPathLength > pathBaseLength && requestPath[pathBaseLength] != '/')
            {
                return null;
            }
            if (!requestPath.StartsWith(_pathBase, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return new RouteData(this, new OwinRouteHandler(_pathBase, requestPath.Substring(pathBaseLength), _appAccessor));
        }

        public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
        {
            return null;
        }
    }
}
