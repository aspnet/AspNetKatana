// <copyright file="DefaultAppLoaderManager.cs" company="Katana contributors">
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
using System.Linq;
using Owin;

namespace Microsoft.Owin.Hosting.Loader
{
    public class DefaultAppLoaderManager : IAppLoaderManager
    {
        private readonly IEnumerable<IAppLoaderProvider> _providers;

        public DefaultAppLoaderManager(IEnumerable<IAppLoaderProvider> providers)
        {
            if (providers == null)
            {
                throw new ArgumentNullException("providers");
            }

            _providers = providers;
        }

        public Action<IAppBuilder> Load(string appName)
        {
            return _providers.Aggregate(
                default(Action<IAppBuilder>),
                (app, loader) => app ?? loader.GetAppLoader().Invoke(appName));
        }
    }
}
