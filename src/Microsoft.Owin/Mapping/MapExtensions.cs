// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Mapping;

namespace Owin
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    /// <summary>
    /// Extension methods for the MapMiddleware
    /// </summary>
    public static class MapExtensions
    {
        /// <summary>
        /// If the request path starts with the given pathMatch, execute the app configured via configuration parameter instead of
        /// continuing to the next component in the pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="pathMatch">The path to match</param>
        /// <param name="configuration">The branch to take for positive path matches</param>
        /// <returns></returns>
        public static IAppBuilder Map(this IAppBuilder app, string pathMatch, Action<IAppBuilder> configuration)
        {
            if (app == null)
            {
                throw new ArgumentNullException("app");
            }
            if (pathMatch == null)
            {
                throw new ArgumentNullException("pathMatch");
            }
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }
            if (!String.IsNullOrEmpty(pathMatch) && pathMatch.EndsWith("/", StringComparison.Ordinal))
            {
                throw new ArgumentException(Resources.Exception_PathMustNotEndWithSlash, "pathMatch");
            }
            return Map(app, new PathString(pathMatch), configuration);
        }

        /// <summary>
        /// If the request path starts with the given pathMatch, execute the app configured via configuration parameter instead of
        /// continuing to the next component in the pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="pathMatch">The path to match</param>
        /// <param name="configuration">The branch to take for positive path matches</param>
        /// <returns></returns>
        public static IAppBuilder Map(this IAppBuilder app, PathString pathMatch, Action<IAppBuilder> configuration)
        {
            if (app == null)
            {
                throw new ArgumentNullException("app");
            }
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }
            if (pathMatch.HasValue && pathMatch.Value.EndsWith("/", StringComparison.Ordinal))
            {
                throw new ArgumentException(Resources.Exception_PathMustNotEndWithSlash, "pathMatch");
            }

            // put middleware in pipeline before creating branch
            var options = new MapOptions { PathMatch = pathMatch };
            IAppBuilder result = app.Use<MapMiddleware>(options);

            // create branch and assign to options
            IAppBuilder branch = app.New();
            configuration(branch);
            options.Branch = (AppFunc)branch.Build(typeof(AppFunc));

            return result;
        }
    }
}
