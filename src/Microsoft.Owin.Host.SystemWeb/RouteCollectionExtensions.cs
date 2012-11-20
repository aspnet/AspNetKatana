// <copyright file="RouteCollectionExtensions.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
// </copyright>
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Routing;
using Owin;

namespace Microsoft.Owin.Host.SystemWeb
{
    /// <summary>
    /// Provides extension methods for registering OWIN applications as System.Web routes.
    /// </summary>
    public static class RouteCollectionExtensions
    {
        /// <summary>
        /// Registers a route for the default OWIN application.
        /// </summary>
        /// <param name="routes">The route collection.</param>
        /// <param name="pathBase">The route path to map to the default OWIN application.</param>
        /// <returns>The created route.</returns>
        public static RouteBase MapOwinRoute(this RouteCollection routes, string pathBase)
        {
            return Add(routes, null, new OwinRoute(pathBase, OwinApplication.Accessor));
        }

        /// <summary>
        /// Registers a route for a specific OWIN application entry point.
        /// </summary>
        /// <typeparam name="TApp">The OWIN application entry point type.</typeparam>
        /// <param name="routes">The route collection.</param>
        /// <param name="pathBase">The route path to map to the given OWIN application.</param>
        /// <param name="app">The OWIN application entry point.</param>
        /// <returns>The created route.</returns>
        public static RouteBase MapOwinRoute<TApp>(this RouteCollection routes, string pathBase, TApp app)
        {
            OwinAppContext appDelegate = OwinBuilder.Build(builder => builder.Run(app));
            return Add(routes, null, new OwinRoute(pathBase, () => appDelegate));
        }

        /// <summary>
        /// Invokes the System.Action startup delegate to build the OWIN application
        /// and then registers a route for it on the given path.
        /// </summary>
        /// <param name="routes">The route collection.</param>
        /// <param name="pathBase">The route path to map to the given OWIN application.</param>
        /// <param name="startup">A System.Action delegate invoked to build the OWIN application.</param>
        /// <returns>The created route.</returns>
        public static RouteBase MapOwinRoute(this RouteCollection routes, string pathBase, Action<IAppBuilder> startup)
        {
            OwinAppContext appDelegate = OwinBuilder.Build(startup);
            return Add(routes, null, new OwinRoute(pathBase, () => appDelegate));
        }

        /// <summary>
        /// Registers a route for the default OWIN application.
        /// </summary>
        /// <param name="routes">The route collection.</param>
        /// <param name="name">The given name of the route.</param>
        /// <param name="pathBase">The route path to map to the default OWIN application.</param>
        /// <returns>The created route.</returns>
        public static RouteBase MapOwinRoute(this RouteCollection routes, string name, string pathBase)
        {
            return Add(routes, name, new OwinRoute(pathBase, OwinApplication.Accessor));
        }

        /// <summary>
        /// Registers a route for a specific OWIN application entry point.
        /// </summary>
        /// <typeparam name="TApp">The OWIN application entry point type.</typeparam>
        /// <param name="routes">The route collection.</param>
        /// <param name="name">The given name of the route.</param>
        /// <param name="pathBase">The route path to map to the given OWIN application.</param>
        /// <param name="app">The OWIN application entry point.</param>
        /// <returns>The created route.</returns>
        public static RouteBase MapOwinRoute<TApp>(this RouteCollection routes, string name, string pathBase, TApp app)
        {
            OwinAppContext appDelegate = OwinBuilder.Build(builder => builder.Run(app));
            return Add(routes, name, new OwinRoute(pathBase, () => appDelegate));
        }

        /// <summary>
        /// Invokes the System.Action startup delegate to build the OWIN application
        /// and then registers a route for it on the given path.
        /// </summary>
        /// <param name="routes">The route collection.</param>
        /// <param name="name">The given name of the route.</param>
        /// <param name="pathBase">The route path to map to the given OWIN application.</param>
        /// <param name="startup">A System.Action delegate invoked to build the OWIN application.</param>
        /// <returns>The created route.</returns>
        public static RouteBase MapOwinRoute(this RouteCollection routes, string name, string pathBase, Action<IAppBuilder> startup)
        {
            OwinAppContext appDelegate = OwinBuilder.Build(startup);
            return Add(routes, name, new OwinRoute(pathBase, () => appDelegate));
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
