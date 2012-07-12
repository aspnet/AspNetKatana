using System.Web.Routing;
using Gate.Builder;
using Owin;
using System;

namespace Katana.Server.AspNet
{
    public static class RouteCollectionExtensions
    {
        public static RouteBase MapOwinRoute(this RouteCollection routes, string pathBase)
        {
            return Add(routes, null, new OwinRoute(pathBase, OwinApplication.Accessor));
        }

        public static RouteBase MapOwinRoute(this RouteCollection routes, string pathBase, AppDelegate app)
        {
            return Add(routes, null, new OwinRoute(pathBase, () => app));
        }

        public static RouteBase MapOwinRoute(this RouteCollection routes, string pathBase, Action<IAppBuilder> configuration)
        {
            return MapOwinRoute(routes, pathBase, AppBuilder.BuildPipeline<AppDelegate>(configuration));
        }

        public static RouteBase MapOwinRoute(this RouteCollection routes, string name, string pathBase)
        {
            return Add(routes, name, new OwinRoute(pathBase, OwinApplication.Accessor));
        }

        public static RouteBase MapOwinRoute(this RouteCollection routes, string name, string pathBase, AppDelegate app)
        {
            return Add(routes, name, new OwinRoute(pathBase, () => app));
        }

        public static RouteBase MapOwinRoute(this RouteCollection routes, string name, string pathBase, Action<IAppBuilder> configuration)
        {
            return MapOwinRoute(routes, name, pathBase, AppBuilder.BuildPipeline<AppDelegate>(configuration));
        }

        private static RouteBase Add(RouteCollection routes, string name, RouteBase item)
        {
            if (string.IsNullOrEmpty(name))
            {
                routes.Add(item);
            }
            else
            {
                routes.Add(name, item);
            }
            return item;
        }
    }
}
