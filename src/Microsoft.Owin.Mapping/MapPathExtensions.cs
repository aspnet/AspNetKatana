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

    public static class MapPathExtensions
    {
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
