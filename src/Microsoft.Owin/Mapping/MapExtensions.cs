// <copyright file="MapExtensions.cs" company="Microsoft Open Technologies, Inc.">
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

using System;
using Microsoft.Owin;
using Microsoft.Owin.Mapping;

namespace Owin
{
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
            if (string.IsNullOrWhiteSpace(pathMatch))
            {
                throw new ArgumentNullException("pathMatch");
            }
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            // put middleware in pipeline before creating branch
            var options = new MapOptions { PathMatch = pathMatch };
            var result = app.Use<MapMiddleware>(options);

            // create branch and assign to options
            IAppBuilder branch = app.New();
            configuration(branch);
            options.Branch = (OwinMiddleware)branch.Build(typeof(OwinMiddleware));

            return result;
        }
    }
}
