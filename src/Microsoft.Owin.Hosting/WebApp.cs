// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.Owin.Hosting.Engine;
using Microsoft.Owin.Hosting.Services;
using Microsoft.Owin.Hosting.Starter;
using Owin;

namespace Microsoft.Owin.Hosting
{
    /// <summary>
    /// These methods are used to load, assemble, and start a web app.
    /// </summary>
    public static class WebApp
    {
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
            return StartImplementation(BuildServices(options), options, startup);
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
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            options.AppStartup = typeof(TStartup).AssemblyQualifiedName;
            return Start(options);
        }

        /// <summary>
        /// Start a web app using the given settings and entry point type, using defaults for items not specified.
        /// </summary>
        /// <returns>An IDisposible instance that can be called to shut down the web app.</returns>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#", Justification = "May contain Uri invalid host characters")]
        public static IDisposable Start(string url)
        {
            if (url == null)
            {
                throw new ArgumentNullException("url");
            }
            return Start(BuildOptions(url));
        }

        /// <summary>
        /// Start a web app using the given settings and entry point type, using defaults for items not specified.
        /// </summary>
        /// <returns>An IDisposible instance that can be called to shut down the web app.</returns>
        public static IDisposable Start(StartOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            return StartImplementation(BuildServices(options), options);
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
