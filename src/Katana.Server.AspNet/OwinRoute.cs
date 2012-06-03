using System;
using System.Web;
using System.Web.Routing;

namespace Katana.Server.AspNet
{
    public class OwinRoute : RouteBase
    {
        private readonly string _basePath;

        public OwinRoute(string basePath)
        {
            _basePath = basePath;
        }

        public override RouteData GetRouteData(HttpContextBase httpContext)
        {
            // First two characters are "~/"
            var requestPath = httpContext.Request.AppRelativeCurrentExecutionFilePath.Substring(2) + httpContext.Request.PathInfo;

            var startsWithBasePath = requestPath.StartsWith(_basePath, StringComparison.OrdinalIgnoreCase);
            return startsWithBasePath ? new RouteData(this, new OwinRouteHandler()) : null;
        }

        public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
        {
            return null;
        }
    }
}