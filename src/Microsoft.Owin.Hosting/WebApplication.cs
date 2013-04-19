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
using System.Diagnostics.CodeAnalysis;
using Microsoft.Owin.Hosting.Engine;
using Microsoft.Owin.Hosting.Services;
using Microsoft.Owin.Hosting.Utilities;
using Owin;

namespace Microsoft.Owin.Hosting
{
    public static class WebApplication
    {
        public static IDisposable Start()
        {
            return Start(BuildOptions());
        }

        public static IDisposable Start(int port)
        {
            return Start(BuildOptions(port));
        }

        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#", Justification = "May contain Uri invalid host characters")]
        public static IDisposable Start(string url)
        {
            return Start(BuildOptions(url));
        }

        public static IDisposable Start(StartOptions options)
        {
            return Start(BuildServices(options), options);
        }

        public static IDisposable Start(Action<IAppBuilder> startup)
        {
            return Start(BuildOptions(), startup);
        }

        public static IDisposable Start(int port, Action<IAppBuilder> startup)
        {
            return Start(BuildOptions(port), startup);
        }

        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#", Justification = "May contain Uri invalid host characters")]
        public static IDisposable Start(string url, Action<IAppBuilder> startup)
        {
            return Start(BuildOptions(url), startup);
        }

        public static IDisposable Start(StartOptions options, Action<IAppBuilder> startup)
        {
            return Start(BuildServices(options), options, startup);
        }

        public static IDisposable Start(IServiceProvider services, StartOptions options)
        {
            return StartImplementation(services, options);
        }

        public static IDisposable Start(IServiceProvider services, StartOptions options, Action<IAppBuilder> startup)
        {
            return StartImplementation(services, options, startup);
        }

        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Non-generic option is available")]
        public static IDisposable Start<TStartup>()
        {
            return Start<TStartup>(BuildOptions());
        }

        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Non-generic option is available")]
        public static IDisposable Start<TStartup>(int port)
        {
            return Start<TStartup>(BuildOptions(port));
        }

        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Non-generic option is available")]
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#", Justification = "Host may be non-Uri compatible value")]
        public static IDisposable Start<TStartup>(string url)
        {
            return Start<TStartup>(BuildOptions(url));
        }

        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Non-generic option is available")]
        public static IDisposable Start<TStartup>(StartOptions options)
        {
            return Start<TStartup>(BuildServices(options), options);
        }

        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Non-generic option is available")]
        public static IDisposable Start<TStartup>(IServiceProvider services, StartOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            options.App = typeof(TStartup).AssemblyQualifiedName;
            return StartImplementation(services, options);
        }

        private static StartOptions BuildOptions()
        {
            return new StartOptions();
        }

        private static StartOptions BuildOptions(int port)
        {
            return new StartOptions() { Port = port };
        }

        private static StartOptions BuildOptions(string url)
        {
            return new StartOptions(url);
        }

        private static IServiceProvider BuildServices(StartOptions options)
        {
            if (options.Settings != null)
            {
                return ServicesFactory.Create(options.Settings);
            }
            return ServicesFactory.Create();
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
