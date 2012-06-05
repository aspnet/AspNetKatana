using System.Web.Routing;
using Gate.Builder;
using Owin;
using System;

namespace Katana.Server.AspNet
{
    public static class RouteCollectionExtensions 
    {
        public static void MapOwinRoute(this RouteCollection routes)
        {
            routes.Add(new OwinRoute(""));
        }

        public static void MapOwinRoute(this RouteCollection routes, AppDelegate app)
        {
            routes.Add(new OwinRoute("", app));
        }

        public static void MapOwinRoute(this RouteCollection routes, Action<IAppBuilder> configuration)
        {
            routes.Add(new OwinRoute("", AppBuilder.BuildConfiguration(configuration)));
        }

        public static void MapOwinRoute(this RouteCollection routes, string pathBase)
        {
            routes.Add(new OwinRoute(pathBase));
        }

        public static void MapOwinRoute(this RouteCollection routes, string pathBase, AppDelegate app)
        {
            routes.Add(new OwinRoute(pathBase, app));
        }

        public static void MapOwinRoute(this RouteCollection routes, string pathBase, Action<IAppBuilder> configuration)
        {
            routes.Add(new OwinRoute(pathBase, AppBuilder.BuildConfiguration(configuration)));
        }
    }
}
