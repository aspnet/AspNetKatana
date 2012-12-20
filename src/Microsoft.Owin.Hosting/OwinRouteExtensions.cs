// <copyright file="RouteExtensions.cs" company="Katana contributors">
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
using Owin;

namespace Microsoft.Owin.Hosting
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    /// <summary>
    /// Provides extension methods for registering OWIN applications as System.Web routes.
    /// </summary>
    public static class OwinRouteExtensions
    {
        public static IAppBuilder MapOwinRoute<TApp>(this IAppBuilder builder, string pathBase, TApp app)
        {
            IAppBuilder branchBuilder = builder.New();
            branchBuilder.Use(app);
            return builder.UseType<OwinRoute>(branchBuilder.Build<AppFunc>(), pathBase);
        }

        public static IAppBuilder MapOwinRoute(this IAppBuilder builder, string pathBase, Action<IAppBuilder> startup)
        {
            IAppBuilder branchBuilder = builder.New();
            startup(branchBuilder);
            return builder.UseType<OwinRoute>(branchBuilder.Build<AppFunc>(), pathBase);
        }
    }
}
