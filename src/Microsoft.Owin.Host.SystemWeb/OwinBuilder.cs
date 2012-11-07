// <copyright file="OwinBuilder.cs" company="Katana contributors">
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
using System.Configuration;
using System.Threading.Tasks;
using Owin;
using Owin.Loader;

namespace Microsoft.Owin.Host.SystemWeb
{
    internal static class OwinBuilder
    {
        internal static OwinAppContext Build()
        {
            string configuration = ConfigurationManager.AppSettings[Constants.OwinConfiguration];
            var loader = new DefaultLoader();
            Action<IAppBuilder> startup = loader.Load(configuration ?? string.Empty);
            return Build(startup);
        }

        internal static OwinAppContext Build(Func<IDictionary<string, object>, Task> app)
        {
            return Build(builder => builder.Run(app));
        }

        internal static OwinAppContext Build(Action<IAppBuilder> startup)
        {
            if (startup == null)
            {
                return null;
            }

            var appContext = new OwinAppContext();
            appContext.Initialize(startup);
            return appContext;
        }
    }
}
