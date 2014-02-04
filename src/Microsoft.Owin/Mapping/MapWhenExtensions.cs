// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
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

            // put middleware in pipeline before creating branch
            var options = new MapWhenOptions { Predicate = predicate };
            IAppBuilder result = app.Use<MapWhenMiddleware>(options);

            // create branch and assign to options
            IAppBuilder branch = app.New();
            configuration(branch);
            options.Branch = (AppFunc)branch.Build(typeof(AppFunc));

            return result;
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

            // put middleware in pipeline before creating branch
            var options = new MapWhenOptions { PredicateAsync = predicate };
            IAppBuilder result = app.Use<MapWhenMiddleware>(options);

            // create branch and assign to options
            IAppBuilder branch = app.New();
            configuration(branch);
            options.Branch = (AppFunc)branch.Build(typeof(AppFunc));

            return result;
        }
    }
}
