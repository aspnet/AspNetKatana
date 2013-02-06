// <copyright file="WebApplication.cs" company="Katana contributors">
//   Copyright 2011-2013 Katana contributors
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
using Owin;
using Microsoft.Owin.Hosting.Services;

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

        public static IDisposable Start(IServiceProvider services, string url)
        {
            return Start(services, BuildOptions(url));
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

        public static IDisposable Start(string url)
        {
            return Start(BuildOptions(url));
        }

        public static IDisposable Start()
        {
            return Start(BuildOptions());
        }

        public static IDisposable Start<TStartup>(IServiceProvider services, StartOptions options)
        {
            options.App = typeof(TStartup).AssemblyQualifiedName;
            return StartImplementation(services, options);
        }

        public static IDisposable Start<TStartup>(IServiceProvider services, Action<StartOptions> configuration)
        {
            return Start<TStartup>(services, BuildOptions(configuration));
        }

        public static IDisposable Start<TStartup>(IServiceProvider services, string url)
        {
            return Start<TStartup>(services, BuildOptions(url));
        }

        public static IDisposable Start<TStartup>(IServiceProvider services)
        {
            return Start<TStartup>(services, BuildOptions());
        }

        public static IDisposable Start<TStartup>(StartOptions options)
        {
            return Start<TStartup>(BuildServices(options), options);
        }

        public static IDisposable Start<TStartup>(Action<StartOptions> configuration)
        {
            return Start<TStartup>(BuildOptions(configuration));
        }

        public static IDisposable Start<TStartup>(string url)
        {
            return Start<TStartup>(BuildOptions(url));
        }

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

        public static IDisposable Start(IServiceProvider services, string url, Action<IAppBuilder> startup)
        {
            return Start(services, BuildOptions(url), startup);
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

        public static IDisposable Start(string url, Action<IAppBuilder> startup)
        {
            return Start(BuildOptions(url), startup);
        }

        public static IDisposable Start(Action<IAppBuilder> startup)
        {
            return Start(BuildOptions(), startup);
        }

        private static StartOptions BuildOptions(string url)
        {
            return new StartOptions { Url = url };
        }

        private static StartOptions BuildOptions()
        {
            return new StartOptions();
        }

        private static StartOptions BuildOptions(Action<StartOptions> configuration)
        {
            var options = new StartOptions();
            configuration(options);
            return options;
        }

        private static IServiceProvider BuildServices(StartOptions options)
        {
            return DefaultServices.Create(options.Services);
        }

        private static IDisposable StartImplementation(IServiceProvider services, StartOptions options)
        {
            var starter = services.GetService<IKatanaStarter>();
            return starter.Start(options);
        }

        private static IDisposable StartImplementation(IServiceProvider services, StartOptions options, Action<IAppBuilder> startup)
        {
            var engine = services.GetService<IKatanaEngine>();
            var context = new StartContext { Options = options, Startup = startup };
            return engine.Start(context);
        }
    }
}
