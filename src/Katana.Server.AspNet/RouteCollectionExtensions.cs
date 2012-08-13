using System.Web.Routing;
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

        public static RouteBase MapOwinRoute<TApp>(this RouteCollection routes, string pathBase, TApp app)
        {
            var appDelegate = OwinBuilder.Build(builder => builder.Run(app));
            return Add(routes, null, new OwinRoute(pathBase, () => appDelegate));
        }

        public static RouteBase MapOwinRoute(this RouteCollection routes, string pathBase, Action<IAppBuilder> startup)
        {
            var appDelegate = OwinBuilder.Build(startup);
            return Add(routes, null, new OwinRoute(pathBase, () => appDelegate));
        }


        public static RouteBase MapOwinRoute(this RouteCollection routes, string name, string pathBase)
        {
            return Add(routes, name, new OwinRoute(pathBase, OwinApplication.Accessor));
        }

        public static RouteBase MapOwinRoute<TApp>(this RouteCollection routes, string name, string pathBase, TApp app)
        {
            var appDelegate = OwinBuilder.Build(builder => builder.Run(app));
            return Add(routes, name, new OwinRoute(pathBase, () => appDelegate));
        }

        public static RouteBase MapOwinRoute(this RouteCollection routes, string name, string pathBase, Action<IAppBuilder> startup)
        {
            var appDelegate = OwinBuilder.Build(startup);
            return Add(routes, null, new OwinRoute(pathBase, () => appDelegate));
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
