using System.Web.Routing;
using Owin;

namespace Katana.Server.AspNet
{
    public static class RouteCollectionExtensions 
    {
        public static void MapOwinRoute(this RouteCollection routes, string pathBase)
        {
            routes.Add(new OwinRoute(pathBase));
        }

        public static void MapOwinRoute(this RouteCollection routes, string pathBase, AppDelegate app)
        {
            routes.Add(new OwinRoute(pathBase, app));
        }
    }
}
