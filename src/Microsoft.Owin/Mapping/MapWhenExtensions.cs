// <copyright file="MapWhenExtensions.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
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
// </copyright>

#if !NET40
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Mapping;

namespace Owin
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    using Predicate = Func<IOwinContext, bool>;
    using PredicateAsync = Func<IOwinContext, Task<bool>>;

    /// <summary>
    /// Extension methods for the MapWhenMiddleware
    /// </summary>
    public static class MapWhenExtensions
    {
        /// <summary>
        /// Branches the request pipeline based on the result of the given predicate.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="predicate">Invoked with the request environment to determine if the branch should be taken</param>
        /// <param name="configuration">Configures a branch to take</param>
        /// <returns></returns>
        public static IAppBuilder MapWhen(this IAppBuilder app, Predicate predicate, Action<IAppBuilder> configuration)
        {
            if (app == null)
            {
                throw new ArgumentNullException("app");
            }
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            IAppBuilder branch = app.New();
            configuration(branch);
            return app.Use<MapWhenMiddleware>(predicate, branch.Build(typeof(OwinMiddleware)));
        }

        /// <summary>
        /// Branches the request pipeline based on the async result of the given predicate.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="predicate">Invoked asynchronously with the request environment to determine if the branch should be taken</param>
        /// <param name="configuration">Configures a branch to take</param>
        /// <returns></returns>
        public static IAppBuilder MapWhenAsync(this IAppBuilder app, PredicateAsync predicate, Action<IAppBuilder> configuration)
        {
            if (app == null)
            {
                throw new ArgumentNullException("app");
            }
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            IAppBuilder branch = app.New();
            configuration(branch);
            return app.Use<MapWhenMiddleware>(predicate, branch.Build(typeof(OwinMiddleware)));
        }
    }
}
#endif
