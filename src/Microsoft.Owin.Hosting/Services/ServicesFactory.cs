// <copyright file="DefaultServices.cs" company="Microsoft Open Technologies, Inc.">
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
using Microsoft.Owin.Hosting.Engine;
using Microsoft.Owin.Hosting.Utilities;

namespace Microsoft.Owin.Hosting.Services
{
    public static class ServicesFactory
    {
        private static readonly Action<ServiceProvider> NoConfiguration = _ => { };

        public static IServiceProvider Create(IDictionary<string, string> settings, Action<ServiceProvider> configuration)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            var services = new ServiceProvider();
            DoCallback(settings, (service, implementation) => services.Add(service, implementation));
            configuration(services);
            return services;
        }

        public static IServiceProvider Create(string settingsFile, Action<ServiceProvider> configuration)
        {
            return Create(SettingsLoader.LoadFromSettingsFile(settingsFile), configuration);
        }

        public static IServiceProvider Create(Action<ServiceProvider> configuration)
        {
            return Create(SettingsLoader.LoadFromConfig(), configuration);
        }

        public static IServiceProvider Create(IDictionary<string, string> settings)
        {
            return Create(settings, NoConfiguration);
        }

        public static IServiceProvider Create(string settingsFile)
        {
            return Create(settingsFile, NoConfiguration);
        }

        public static IServiceProvider Create()
        {
            return Create(NoConfiguration);
        }

        public static void ForEach(IDictionary<string, string> settings, Action<Type, Type> callback)
        {
            DoCallback(settings, callback);
        }

        public static void ForEach(string settingsFile, Action<Type, Type> callback)
        {
            DoCallback(SettingsLoader.LoadFromSettingsFile(settingsFile), callback);
        }

        public static void ForEach(Action<Type, Type> callback)
        {
            DoCallback(SettingsLoader.LoadFromConfig(), callback);
        }

        private static void DoCallback(IDictionary<string, string> settings, Action<Type, Type> callback)
        {
            DoCallback((service, implementation) =>
            {
                string replacementNames;
                if (settings.TryGetValue(service.FullName, out replacementNames))
                {
                    foreach (var replacementName in replacementNames.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        Type replacement = Type.GetType(replacementName);
                        callback(service, replacement);
                    }
                }
                else
                {
                    callback(service, implementation);
                }
            });
        }

        private static void DoCallback(Action<Type, Type> callback)
        {
            callback(typeof(IHostingStarter), typeof(HostingStarter));
            callback(typeof(IHostingStarterFactory), typeof(HostingStarterFactory));
            callback(typeof(IHostingStarterActivator), typeof(HostingStarterActivator));
            callback(typeof(IHostingEngine), typeof(HostingEngine));
            callback(typeof(ITraceOutputFactory), typeof(TraceOutputFactory));
            callback(typeof(IAppLoader), typeof(AppLoader));
            callback(typeof(IAppLoaderFactory), typeof(AppLoaderFactory));
            callback(typeof(IAppActivator), typeof(AppActivator));
            callback(typeof(IAppBuilderFactory), typeof(AppBuilderFactory));
            callback(typeof(IServerFactoryLoader), typeof(ServerFactoryLoader));
            callback(typeof(IServerFactoryActivator), typeof(ServerFactoryActivator));
        }
    }
}
