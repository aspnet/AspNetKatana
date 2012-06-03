using System.Web;
using System.Web.Routing;

namespace Katana.Server.AspNet
{
    public class OwinRouteHandler : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            throw new System.NotImplementedException();
        }
    }
}