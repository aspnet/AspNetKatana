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
        public interface IDefaultServiceCallback
        {
            void Add<TService, TClass>() where TClass : TService;
        }

        public static IServiceProvider Create()
        {
            return Create(_ => { });
        }

        public static IServiceProvider Create(Action<DefaultServiceProvider> configuration)
        {
            var services = new DefaultServiceProvider();
            ForEach((service, implementation) => services.Add(service, implementation));
            configuration(services);
            return services;
        }

        public static void ForEach(Action<Type, Type> callback)
        {
            ForEach(new SimpleCallback(callback));
        }

        public static void ForEach(IDefaultServiceCallback callback)
        {
            callback.Add<IKatanaStarter, KatanaStarter>();
            callback.Add<IHostingStarterFactory, DefaultHostingStarterFactory>();
            callback.Add<IHostingStarterActivator, DefaultHostingStarterActivator>();
            callback.Add<IKatanaEngine, KatanaEngine>();
            callback.Add<IKatanaSettingsProvider, DefaultKatanaSettingsProvider>();
            callback.Add<ITraceOutputBinder, DefaultTraceOutputBinder>();
            callback.Add<IAppLoaderManager, DefaultAppLoaderManager>();
            callback.Add<IAppLoaderProvider, DefaultAppLoaderProvider>();
            callback.Add<IAppActivator, DefaultAppActivator>();
            callback.Add<IAppBuilderFactory, DefaultAppBuilderFactory>();
        }

        private class SimpleCallback : IDefaultServiceCallback
        {
            private readonly Action<Type, Type> _callback;

            public SimpleCallback(Action<Type, Type> callback)
            {
                _callback = callback;
            }

            public void Add<TService, TClass>() where TClass : TService
            {
                _callback.Invoke(typeof(TService), typeof(TClass));
            }
        }
    }
}
