using System;
using System.Web;
using System.Web.Routing;
using Owin;

namespace Katana.Server.AspNet
{
    public class OwinRoute : RouteBase
    {
        private readonly string _basePath;
        private readonly Func<AppDelegate> _appAccessor;

        public OwinRoute(string basePath)
        {
            _basePath = basePath;
        }

        public OwinRoute(string basePath, AppDelegate app)
            : this(basePath, () => app)
        {
        }

        public OwinRoute(string basePath, Func<AppDelegate> appAccessor)
        {
            _basePath = basePath;
            _appAccessor = appAccessor;
        }

        public override RouteData GetRouteData(HttpContextBase httpContext)
        {
            // First two characters are "~/"
            var requestPath = httpContext.Request.AppRelativeCurrentExecutionFilePath.Substring(2) + httpContext.Request.PathInfo;

            var startsWithBasePath = requestPath.StartsWith(_basePath, StringComparison.OrdinalIgnoreCase);
            return startsWithBasePath ? new RouteData(this, new OwinRouteHandler(_appAccessor)) : null;
        }

        public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
        {
            return null;
        }
    }
}