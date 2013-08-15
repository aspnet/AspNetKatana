// <copyright file="WebApp.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Globalization;
using Microsoft.Owin.Hosting.Engine;
using Microsoft.Owin.Hosting.Services;
using Microsoft.Owin.Hosting.Starter;
using Microsoft.Owin.Hosting.Utilities;
using Owin;

namespace Microsoft.Owin.Hosting
{
    /// <summary>
    /// These methods are used to load, assemble, and start a web app.
    /// </summary>
    public static class WebApp
    {
        /// <summary>
        /// Start a web app with the given options, using defaults for items not specified.
        /// </summary>
        /// <returns>An IDisposible instance that can be called to shut down the web app.</returns>
        public static IDisposable Start(StartOptions options)
        {
            return Start(BuildServices(options), options);
        }

        /// <summary>
        /// Start a web app using default settings and the given port and entry point.
        /// e.g. Discover the ServerFactory and run at http://localhost:{port}/.
        /// </summary>
        /// <returns>An IDisposible instance that can be called to shut down the web app.</returns>
        public static IDisposable Start(int port, Action<IAppBuilder> startup)
        {
            return Start(BuildOptions(port), startup);
        }

        /// <summary>
        /// Start a web app using default settings and the given url and entry point.
        /// e.g. Discover the ServerFactory and run at the given url.
        /// </summary>
        /// <returns>An IDisposible instance that can be called to shut down the web app.</returns>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#", Justification = "May contain Uri invalid host characters")]
        public static IDisposable Start(string url, Action<IAppBuilder> startup)
        {
            return Start(BuildOptions(url), startup);
        }

        /// <summary>
        /// Start a web app using the given settings and entry point, using defaults for items not specified.
        /// </summary>
        /// <returns>An IDisposible instance that can be called to shut down the web app.</returns>
        public static IDisposable Start(StartOptions options, Action<IAppBuilder> startup)
        {
            return Start(BuildServices(options), options, startup);
        }

        /// <summary>
        /// Start a web app with the given service provider and options, using defaults for items not specified.
        /// </summary>
        /// <returns>An IDisposible instance that can be called to shut down the web app.</returns>
        public static IDisposable Start(IServiceProvider services, StartOptions options)
        {
            return StartImplementation(services, options);
        }

        /// <summary>
        /// Start a web app using the given service provider, settings, and entry point, using defaults for items not specified.
        /// </summary>
        /// <returns>An IDisposible instance that can be called to shut down the web app.</returns>
        public static IDisposable Start(IServiceProvider services, StartOptions options, Action<IAppBuilder> startup)
        {
            return StartImplementation(services, options, startup);
        }

        /// <summary>
        /// Start a web app using default settings and the given port and entry point type.
        /// e.g. Discover the ServerFactory and run at http://localhost:{port}/.
        /// </summary>
        /// <returns>An IDisposible instance that can be called to shut down the web app.</returns>
        public static IDisposable Start<TStartup>(int port)
        {
            return Start<TStartup>(BuildOptions(port));
        }

        /// <summary>
        /// Start a web app using default settings and the given url and entry point type.
        /// e.g. Discover the ServerFactory and run at the given url.
        /// </summary>
        /// <returns>An IDisposible instance that can be called to shut down the web app.</returns>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#", Justification = "Host may be non-Uri compatible value")]
        public static IDisposable Start<TStartup>(string url)
        {
            return Start<TStartup>(BuildOptions(url));
        }

        /// <summary>
        /// Start a web app using the given settings and entry point type, using defaults for items not specified.
        /// </summary>
        /// <returns>An IDisposible instance that can be called to shut down the web app.</returns>
        public static IDisposable Start<TStartup>(StartOptions options)
        {
            return Start<TStartup>(BuildServices(options), options);
        }

        /// <summary>
        /// Start a web app using the given service provider, settings, and entry point type, using defaults for items not specified.
        /// </summary>
        /// <returns>An IDisposible instance that can be called to shut down the web app.</returns>
        public static IDisposable Start<TStartup>(IServiceProvider services, StartOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            options.AppStartup = typeof(TStartup).AssemblyQualifiedName;
            return StartImplementation(services, options);
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
            if (services == null)
            {
                throw new ArgumentNullException("services");
            }
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            var starter = services.GetService<IHostingStarter>();
            if (starter == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
                    Resources.Exception_FailedToResolveService, "IHostingStarter"));
            }
            return starter.Start(options);
        }

        private static IDisposable StartImplementation(IServiceProvider services, StartOptions options, Action<IAppBuilder> startup)
        {
            if (services == null)
            {
                throw new ArgumentNullException("services");
            }
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            if (startup == null)
            {
                throw new ArgumentNullException("startup");
            }

            if (string.IsNullOrWhiteSpace(options.AppStartup))
            {
                // Populate AppStartup for use in host.AppName
                options.AppStartup = startup.Method.ReflectedType.FullName;
            }

            var engine = services.GetService<IHostingEngine>();
            if (engine == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
                    Resources.Exception_FailedToResolveService, "IHostingEngine"));
            }
            var context = new StartContext(options);
            context.Startup = startup;
            return engine.Start(context);
        }
    }
}
