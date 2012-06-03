using System.Web.Routing;

namespace Katana.Server.AspNet
{
    public static class RouteCollectionExtensions 
    {
        public static void AddOwinRoute(this RouteCollection routes, string basePath)
        {
            routes.Add(new OwinRoute(basePath));
        }
    }
}
