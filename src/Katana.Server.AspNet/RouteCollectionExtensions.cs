using System.Web.Routing;
using Owin;

namespace Katana.Server.AspNet
{
    public static class RouteCollectionExtensions 
    {
        public static void AddOwinRoute(this RouteCollection routes, string basePath)
        {
            routes.Add(new OwinRoute(basePath));
        }

        public static void AddOwinRoute(this RouteCollection routes, string basePath, AppDelegate app)
        {
            routes.Add(new OwinRoute(basePath, app));
        }
    }
}
