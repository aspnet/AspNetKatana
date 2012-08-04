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

        public static RouteBase MapOwinRoute(this RouteCollection routes, string pathBase, Action<IAppBuilder> startup)
        {
            return MapOwinRoute(routes, pathBase, OwinBuilder.Build(startup));
        }

        public static RouteBase MapOwinRoute(this RouteCollection routes, string name, string pathBase)
        {
            return Add(routes, name, new OwinRoute(pathBase, OwinApplication.Accessor));
        }

        public static RouteBase MapOwinRoute(this RouteCollection routes, string name, string pathBase, AppDelegate app)
        {
            return Add(routes, name, new OwinRoute(pathBase, () => app));
        }

        public static RouteBase MapOwinRoute(this RouteCollection routes, string name, string pathBase, Action<IAppBuilder> startup)
        {
            return MapOwinRoute(routes, name, pathBase, OwinBuilder.Build(startup));
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
