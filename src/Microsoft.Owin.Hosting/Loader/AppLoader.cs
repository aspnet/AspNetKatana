// <copyright file="AppLoader.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Collections.Generic;
using System.Linq;
using Owin;

namespace Microsoft.Owin.Hosting.Loader
{
    public class AppLoader : IAppLoader
    {
        private readonly IEnumerable<IAppLoaderFactory> _providers;

        public AppLoader(IEnumerable<IAppLoaderFactory> providers)
        {
            if (providers == null)
            {
                throw new ArgumentNullException("providers");
            }

            _providers = providers;
        }

        public Action<IAppBuilder> Load(string appName)
        {
            Func<string, Action<IAppBuilder>> chain = _providers.Aggregate(
                (Func<string, Action<IAppBuilder>>)(arg => null),
                (next, provider) => provider.Create(next));

            return chain.Invoke(appName);
        }
    }
}
