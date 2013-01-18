// <copyright file="MapPathExtensions.cs" company="Katana contributors">
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

namespace Microsoft.Owin.Mapping
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    /// <summary>
    /// Extension methods for the MapPathMiddleware
    /// </summary>
    public static class MapPathExtensions
    {
        /// <summary>
        /// If the request path starts with the given pathBase, execute the given branchApp instead of 
        /// continuing to the next component in the pipeline.
        /// </summary>
        /// <typeparam name="TApp">The application signature</typeparam>
        /// <param name="builder"></param>
        /// <param name="pathBase">The path to match</param>
        /// <param name="branchApp">The branch to take for positive path matches</param>
        /// <returns></returns>
        public static IAppBuilder MapPath<TApp>(this IAppBuilder builder, string pathBase, TApp branchApp)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }
            if (string.IsNullOrWhiteSpace(pathBase))
            {
                throw new ArgumentNullException("pathBase");
            }
            if (branchApp == null)
            {
                throw new ArgumentNullException("branchApp");
            }

            IAppBuilder branchBuilder = builder.New();
            branchBuilder.Run(branchApp);
            return builder.UseType<MapPathMiddleware>(branchBuilder.Build<AppFunc>(), pathBase);
        }

        /// <summary>
        /// If the request path starts with the given pathBase, execute the app configured via branchConfig instead of 
        /// continuing to the next component in the pipeline.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="pathBase">The path to match</param>
        /// <param name="branchConfig">The branch to take for positive path matches</param>
        /// <returns></returns>
        public static IAppBuilder MapPath(this IAppBuilder builder, string pathBase, Action<IAppBuilder> branchConfig)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }
            if (string.IsNullOrWhiteSpace(pathBase))
            {
                throw new ArgumentNullException("pathBase");
            }
            if (branchConfig == null)
            {
                throw new ArgumentNullException("branchConfig");
            }

            IAppBuilder branchBuilder = builder.New();
            branchConfig(branchBuilder);
            return builder.UseType<MapPathMiddleware>(branchBuilder.Build<AppFunc>(), pathBase);
        }
    }
}
