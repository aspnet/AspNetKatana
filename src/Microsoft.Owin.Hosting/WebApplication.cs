// <copyright file="WebApplication.cs" company="Microsoft Open Technologies, Inc.">
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
using Microsoft.Owin.Hosting.Engine;
using Microsoft.Owin.Hosting.Services;
using Microsoft.Owin.Hosting.Settings;
using Microsoft.Owin.Hosting.Starter;
using Owin;

namespace Microsoft.Owin.Hosting
{
    public static class WebApplication
    {
        public static IDisposable Start(IServiceProvider services, StartOptions options)
        {
            return StartImplementation(services, options);
        }

        public static IDisposable Start(IServiceProvider services, Action<StartOptions> configuration)
        {
            return Start(services, BuildOptions(configuration));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "1#", Justification = "May contain Uri invalid host characters")]
        public static IDisposable Start(IServiceProvider services, string url)
        {
            return Start(services, BuildOptions(url));
        }

        public static IDisposable Start(IServiceProvider services, int port)
        {
            return Start(services, BuildOptions(port));
        }

        public static IDisposable Start(IServiceProvider services)
        {
            return Start(services, BuildOptions());
        }

        public static IDisposable Start(StartOptions options)
        {
            return Start(BuildServices(options), options);
        }

        public static IDisposable Start(Action<StartOptions> configuration)
        {
            return Start(BuildOptions(configuration));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#", Justification = "May contain Uri invalid host characters")]
        public static IDisposable Start(string url)
        {
            return Start(BuildOptions(url));
        }

        public static IDisposable Start(int port)
        {
            return Start(BuildOptions(port));
        }

        public static IDisposable Start()
        {
            return Start(BuildOptions());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Non-generic option is available")]
        public static IDisposable Start<TStartup>(IServiceProvider services, StartOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            options.App = typeof(TStartup).AssemblyQualifiedName;
            return StartImplementation(services, options);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Non-generic option is available")]
        public static IDisposable Start<TStartup>(IServiceProvider services, Action<StartOptions> configuration)
        {
            return Start<TStartup>(services, BuildOptions(configuration));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Non-generic option is available")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "1#", Justification = "May contain Uri invalid host characters")]
        public static IDisposable Start<TStartup>(IServiceProvider services, string url)
        {
            return Start<TStartup>(services, BuildOptions(url));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Non-generic option is available")]
        public static IDisposable Start<TStartup>(IServiceProvider services, int port)
        {
            return Start<TStartup>(services, BuildOptions(port));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Non-generic option is available")]
        public static IDisposable Start<TStartup>(IServiceProvider services)
        {
            return Start<TStartup>(services, BuildOptions());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Non-generic option is available")]
        public static IDisposable Start<TStartup>(StartOptions options)
        {
            return Start<TStartup>(BuildServices(options), options);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Non-generic option is available")]
        public static IDisposable Start<TStartup>(Action<StartOptions> configuration)
        {
            return Start<TStartup>(BuildOptions(configuration));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Non-generic option is available")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#", Justification = "Host may be non-Uri compatible value")]
        public static IDisposable Start<TStartup>(string url)
        {
            return Start<TStartup>(BuildOptions(url));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Non-generic option is available")]
        public static IDisposable Start<TStartup>(int port)
        {
            return Start<TStartup>(BuildOptions(port));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Non-generic option is available")]
        public static IDisposable Start<TStartup>()
        {
            return Start<TStartup>(BuildOptions());
        }

        public static IDisposable Start(IServiceProvider services, StartOptions options, Action<IAppBuilder> startup)
        {
            return StartImplementation(services, options, startup);
        }

        public static IDisposable Start(IServiceProvider services, Action<StartOptions> configuration, Action<IAppBuilder> startup)
        {
            return Start(services, BuildOptions(configuration), startup);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "1#", Justification = "May contain Uri invalid host characters")]
        public static IDisposable Start(IServiceProvider services, string url, Action<IAppBuilder> startup)
        {
            return Start(services, BuildOptions(url), startup);
        }

        public static IDisposable Start(IServiceProvider services, int port, Action<IAppBuilder> startup)
        {
            return Start(services, BuildOptions(port), startup);
        }

        public static IDisposable Start(IServiceProvider services, Action<IAppBuilder> startup)
        {
            return Start(services, BuildOptions(), startup);
        }

        public static IDisposable Start(StartOptions options, Action<IAppBuilder> startup)
        {
            return Start(BuildServices(options), options, startup);
        }

        public static IDisposable Start(Action<StartOptions> configuration, Action<IAppBuilder> startup)
        {
            return Start(BuildOptions(configuration), startup);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#", Justification = "May contain Uri invalid host characters")]
        public static IDisposable Start(string url, Action<IAppBuilder> startup)
        {
            return Start(BuildOptions(url), startup);
        }

        public static IDisposable Start(int port, Action<IAppBuilder> startup)
        {
            return Start(BuildOptions(port), startup);
        }

        public static IDisposable Start(Action<IAppBuilder> startup)
        {
            return Start(BuildOptions(), startup);
        }

        private static StartOptions BuildOptions(string url)
        {
            return BuildOptions(options => options.Urls.Add(url));
        }

        private static StartOptions BuildOptions(int port)
        {
            return BuildOptions(options => options.Port = port);
        }

        private static StartOptions BuildOptions()
        {
            return BuildOptions(options => { });
        }

        private static StartOptions BuildOptions(Action<StartOptions> configuration)
        {
            var options = new StartOptions();
            configuration(options);
            return options;
        }

        private static IServiceProvider BuildServices(StartOptions options)
        {
            if (options.Settings != null)
            {
                return DefaultServices.Create(options.Settings);
            }
            return DefaultServices.Create();
        }

        private static IDisposable StartImplementation(IServiceProvider services, StartOptions options)
        {
            var starter = services.GetService<IHostingStarter>();
            return starter.Start(options);
        }

        private static IDisposable StartImplementation(IServiceProvider services, StartOptions options, Action<IAppBuilder> startup)
        {
            var engine = services.GetService<IHostingEngine>();
            var context = StartContext.Create(options);
            context.Startup = startup;
            return engine.Start(context);
        }
    }
}
