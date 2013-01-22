// <copyright file="DefaultServices.cs" company="Katana contributors">
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
using Microsoft.Owin.Hosting.Builder;
using Microsoft.Owin.Hosting.Loader;
using Microsoft.Owin.Hosting.Settings;
using Microsoft.Owin.Hosting.Starter;
using Microsoft.Owin.Hosting.Tracing;

namespace Microsoft.Owin.Hosting.Services
{
    public static class DefaultServices
    {
        public static IServiceProvider Create()
        {
            return Create(_ => { });
        }

        public static IServiceProvider Create(Action<DefaultServiceProvider> configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            var services = new DefaultServiceProvider();
            ForEach((service, implementation) => services.Add(service, implementation));
            configuration(services);
            return services;
        }

        public static void ForEach(Action<Type, Type> callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }

            callback(typeof(IKatanaStarter), typeof(KatanaStarter));
            callback(typeof(IHostingStarterFactory), typeof(DefaultHostingStarterFactory));
            callback(typeof(IHostingStarterActivator), typeof(DefaultHostingStarterActivator));
            callback(typeof(IKatanaEngine), typeof(KatanaEngine));
            callback(typeof(IKatanaSettingsProvider), typeof(DefaultKatanaSettingsProvider));
            callback(typeof(ITraceOutputBinder), typeof(DefaultTraceOutputBinder));
            callback(typeof(IAppLoaderManager), typeof(DefaultAppLoaderManager));
            callback(typeof(IAppLoaderProvider), typeof(DefaultAppLoaderProvider));
            callback(typeof(IAppActivator), typeof(DefaultAppActivator));
            callback(typeof(IAppBuilderFactory), typeof(DefaultAppBuilderFactory));
        }
    }
}
