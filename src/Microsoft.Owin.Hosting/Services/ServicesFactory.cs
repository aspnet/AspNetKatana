// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Owin.Hosting.Builder;
using Microsoft.Owin.Hosting.Engine;
using Microsoft.Owin.Hosting.Loader;
using Microsoft.Owin.Hosting.ServerFactory;
using Microsoft.Owin.Hosting.Starter;
using Microsoft.Owin.Hosting.Tracing;
using Microsoft.Owin.Hosting.Utilities;
using Microsoft.Owin.Logging;

namespace Microsoft.Owin.Hosting.Services
{
    /// <summary>
    /// Create a default ServiceProvider with input from a variety or sources.
    /// </summary>
    public static class ServicesFactory
    {
        private static readonly Action<ServiceProvider> NoConfiguration = _ => { };

        /// <summary>
        /// Create a default ServiceProvider with the given settings.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Create a default ServiceProvider with the given settings file.
        /// </summary>
        /// <param name="settingsFile"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceProvider Create(string settingsFile, Action<ServiceProvider> configuration)
        {
            return Create(SettingsLoader.LoadFromSettingsFile(settingsFile), configuration);
        }

        /// <summary>
        /// Create a default ServiceProvider.
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceProvider Create(Action<ServiceProvider> configuration)
        {
            return Create(SettingsLoader.LoadFromConfig(), configuration);
        }

        /// <summary>
        /// Create a default ServiceProvider with the given settings.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static IServiceProvider Create(IDictionary<string, string> settings)
        {
            return Create(settings, NoConfiguration);
        }

        /// <summary>
        /// Create a default ServiceProvider with the given settings file.
        /// </summary>
        /// <param name="settingsFile"></param>
        /// <returns></returns>
        public static IServiceProvider Create(string settingsFile)
        {
            return Create(settingsFile, NoConfiguration);
        }

        /// <summary>
        /// Create a default ServiceProvider.
        /// </summary>
        /// <returns></returns>
        public static IServiceProvider Create()
        {
            return Create(NoConfiguration);
        }

        /// <summary>
        /// Enumerate the default service types with the given settings overrides.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="callback"></param>
        public static void ForEach(IDictionary<string, string> settings, Action<Type, Type> callback)
        {
            DoCallback(settings, callback);
        }

        /// <summary>
        /// Enumerate the default service types with the given settings file overrides.
        /// </summary>
        /// <param name="settingsFile"></param>
        /// <param name="callback"></param>
        public static void ForEach(string settingsFile, Action<Type, Type> callback)
        {
            DoCallback(SettingsLoader.LoadFromSettingsFile(settingsFile), callback);
        }

        /// <summary>
        /// Enumerate the default service types.
        /// </summary>
        /// <param name="callback"></param>
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
            callback(typeof(ILoggerFactory), typeof(InjectableDiagnosticsLoggerFactory));
        }

        // StructureMap can't handle DiagnosticsLoggerFactory because it has two constructors
        // and one of them has unregistered types. Make it to use the other constructor.
        private class InjectableDiagnosticsLoggerFactory : DiagnosticsLoggerFactory
        {
            public InjectableDiagnosticsLoggerFactory()
                : base()
            {
            }
        }
    }
}
