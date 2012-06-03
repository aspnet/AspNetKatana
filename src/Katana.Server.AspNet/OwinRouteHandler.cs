using System;
using System.Web;
using System.Web.Routing;
using Owin;

namespace Katana.Server.AspNet
{
    public class OwinRouteHandler : IRouteHandler
    {
        private readonly Func<AppDelegate> _appAccessor;

        public OwinRouteHandler()
        {
        }

        public OwinRouteHandler(Func<AppDelegate> appAccessor)
        {
            _appAccessor = appAccessor;
        }

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return new OwinHttpHandler(_appAccessor);
        }
    }
}
