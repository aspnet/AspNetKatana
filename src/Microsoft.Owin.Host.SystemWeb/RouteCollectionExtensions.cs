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
    public static class RouteCollectionExtensions
    {
        public static RouteBase MapOwinRoute(this RouteCollection routes, string pathBase)
        {
            return Add(routes, null, new OwinRoute(pathBase, OwinApplication.Accessor));
        }

        public static RouteBase MapOwinRoute<TApp>(this RouteCollection routes, string pathBase, TApp app)
        {
            OwinAppContext appDelegate = OwinBuilder.Build(builder => builder.Run(app));
            return Add(routes, null, new OwinRoute(pathBase, () => appDelegate));
        }

        public static RouteBase MapOwinRoute(this RouteCollection routes, string pathBase, Action<IAppBuilder> startup)
        {
            OwinAppContext appDelegate = OwinBuilder.Build(startup);
            return Add(routes, null, new OwinRoute(pathBase, () => appDelegate));
        }

        public static RouteBase MapOwinRoute(this RouteCollection routes, string name, string pathBase)
        {
            return Add(routes, name, new OwinRoute(pathBase, OwinApplication.Accessor));
        }

        public static RouteBase MapOwinRoute<TApp>(this RouteCollection routes, string name, string pathBase, TApp app)
        {
            OwinAppContext appDelegate = OwinBuilder.Build(builder => builder.Run(app));
            return Add(routes, name, new OwinRoute(pathBase, () => appDelegate));
        }

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
