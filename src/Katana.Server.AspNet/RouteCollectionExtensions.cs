using System.Web.Routing;
using Gate.Builder;
using Owin;
using System;

namespace Katana.Server.AspNet
{
    public static class RouteCollectionExtensions
    {
        //public static RouteBase MapOwinRoute(this RouteCollection routes)
        //{
        //    return Add(routes, null, new OwinRoute(""));
        //}

        //public static RouteBase MapOwinRoute(this RouteCollection routes, AppDelegate app)
        //{
        //    return Add(routes, null, new OwinRoute("", app));
        //}

        //public static RouteBase MapOwinRoute(this RouteCollection routes, Action<IAppBuilder> configuration)
        //{
        //    return Add(routes, null, new OwinRoute("", AppBuilder.BuildConfiguration(configuration)));
        //}

        //public static RouteBase MapOwinRoute(this RouteCollection routes, string name, Action<IAppBuilder> configuration)
        //{
        //    return Add(routes, null, new OwinRoute(name, AppBuilder.BuildConfiguration(configuration)));
        //}

        public static RouteBase MapOwinRoute(this RouteCollection routes, string pathBase)
        {
            return Add(routes, null, new OwinRoute(pathBase));
        }

        public static RouteBase MapOwinRoute(this RouteCollection routes, string name, string pathBase)
        {
            return Add(routes, name, new OwinRoute(pathBase));
        }

        public static RouteBase MapOwinRoute(this RouteCollection routes, string pathBase, AppDelegate app)
        {
            return Add(routes, null, new OwinRoute(pathBase, app));
        }

        public static RouteBase MapOwinRoute(this RouteCollection routes, string name, string pathBase, AppDelegate app)
        {
            return Add(routes, name, new OwinRoute(pathBase, app));
        }

        public static RouteBase MapOwinRoute(this RouteCollection routes, string pathBase, Action<IAppBuilder> configuration)
        {
            return Add(routes, null, new OwinRoute(pathBase, AppBuilder.BuildConfiguration(configuration)));
        }

        public static RouteBase MapOwinRoute(this RouteCollection routes, string name, string pathBase, Action<IAppBuilder> configuration)
        {
            return Add(routes, name, new OwinRoute(pathBase, AppBuilder.BuildConfiguration(configuration)));
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
