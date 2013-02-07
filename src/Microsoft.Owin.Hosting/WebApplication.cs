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
using System.Diagnostics.CodeAnalysis;
using Microsoft.Owin.Hosting.Services;
using Owin;

namespace Microsoft.Owin.Hosting
{
    public static class WebApplication
    {
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#", Justification = "By design")]
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Would require too many overloads")]
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "By design")]
        public static IDisposable Start<TStartup>(
            string url = null,
            string server = null,
            string boot = null,
            string outputFile = null,
            int verbosity = 0,
            IServiceProvider services = null)
        {
            return Start(
                services,
                new StartOptions
                {
                    Boot = boot,
                    Server = server,
                    App = typeof(TStartup).AssemblyQualifiedName,
                    OutputFile = outputFile,
                    Verbosity = verbosity,
                    Url = url,
                });
        }

        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "1#", Justification = "By design")]
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Would require too many overloads")]
        public static IDisposable Start(
            string app = null,
            string url = null,
            string server = null,
            string boot = null,
            string outputFile = null,
            int verbosity = 0,
            IServiceProvider services = null)
        {
            return Start(
                services,
                new StartOptions
                {
                    Boot = boot,
                    Server = server,
                    App = app,
                    OutputFile = outputFile,
                    Verbosity = verbosity,
                    Url = url,
                });
        }

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Pass through")]
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "1#", Justification = "By design")]
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Would require too many overloads")]
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "By design")]
        public static IDisposable Start<TStartup>(
            this IKatanaStarter starter,
            string url = null,
            string server = null,
            string scheme = null,
            string host = null,
            int? port = null,
            string path = null,
            string boot = null,
            string outputFile = null,
            int verbosity = 0)
        {
            return starter.Start(
                new StartOptions
                {
                    Boot = boot,
                    Server = server,
                    App = typeof(TStartup).AssemblyQualifiedName,
                    OutputFile = outputFile,
                    Verbosity = verbosity,
                    Url = url,
                });
        }

        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Pass through")]
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "2#", Justification = "By design")]
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Would require too many overloads")]
        public static IDisposable Start(
            this IKatanaStarter starter,
            string app = null,
            string url = null,
            string server = null,
            string boot = null,
            string outputFile = null,
            int verbosity = 0)
        {
            return starter.Start(
                new StartOptions
                {
                    Boot = boot,
                    Server = server,
                    App = app,
                    OutputFile = outputFile,
                    Verbosity = verbosity,
                    Url = url,
                });
        }

        public static IDisposable Start(IServiceProvider services, StartOptions options)
        {
            return StartImplementation(services, options);
        }

        public static IDisposable Start(IServiceProvider services, string url)
        {
            return Start(services, new StartOptions { Url = url });
        }

        public static IDisposable Start(IServiceProvider services, Action<StartOptions> configuration)
        {
            var options = new StartOptions();
            configuration(options);
            return Start(services, options);
        }

        public static IDisposable Start(StartOptions options)
        {
            return Start(DefaultServices.Create(), options);
        }

        public static IDisposable Start(string url)
        {
            return Start(DefaultServices.Create(), url);
        }

        public static IDisposable Start(Action<StartOptions> configuration)
        {
            return Start(DefaultServices.Create(), configuration);
        }

        public static IDisposable Start<TStartup>(IServiceProvider services, StartOptions options)
        {
            options.App = typeof(TStartup).AssemblyQualifiedName;
            return StartImplementation(services, options);
        }

        public static IDisposable Start<TStartup>(IServiceProvider services, string url)
        {
            return Start<TStartup>(services, new StartOptions { Url = url });
        }

        public static IDisposable Start<TStartup>(IServiceProvider services, Action<StartOptions> configuration)
        {
            var options = new StartOptions();
            configuration(options);
            return Start<TStartup>(services, options);
        }

        public static IDisposable Start<TStartup>(StartOptions options)
        {
            return Start<TStartup>(DefaultServices.Create(), options);
        }

        public static IDisposable Start<TStartup>(string url)
        {
            return Start<TStartup>(DefaultServices.Create(), url);
        }

        public static IDisposable Start<TStartup>(Action<StartOptions> configuration)
        {
            return Start<TStartup>(DefaultServices.Create(), configuration);
        }

        public static IDisposable Start(IServiceProvider services, StartOptions options, Action<IAppBuilder> startup)
        {
            return StartImplementation(services, options);
        }

        public static IDisposable Start(IServiceProvider services, string url, Action<IAppBuilder> startup)
        {
            return Start(services, new StartOptions { Url = url });
        }

        public static IDisposable Start(IServiceProvider services, Action<StartOptions> configuration, Action<IAppBuilder> startup)
        {
            var options = new StartOptions();
            configuration(options);
            return Start(services, options);
        }

        public static IDisposable Start(StartOptions options, Action<IAppBuilder> startup)
        {
            return Start(DefaultServices.Create(), options, startup);
        }

        public static IDisposable Start(string url, Action<IAppBuilder> startup)
        {
            return Start(DefaultServices.Create(), url, startup);
        }

        public static IDisposable Start(Action<StartOptions> configuration, Action<IAppBuilder> startup)
        {
            return Start(DefaultServices.Create(), configuration, startup);
        }
        
        
        private static IDisposable StartImplementation(IServiceProvider services, StartOptions options)
        {
            var starter = services.GetService<IKatanaStarter>();
            return starter.Start(options);
        }

        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#", Justification = "By design")]
        public static IDisposable Start(string url, Action<IAppBuilder> startup)
        {
            IServiceProvider services = DefaultServices.Create();
            return Start(url, services, startup);
        }

        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#", Justification = "By design")]
        public static IDisposable Start(string url, IServiceProvider services, Action<IAppBuilder> startup)
        {
            IKatanaEngine engine = services.GetService<IKatanaEngine>();
            StartContext context = new StartContext();
            context.Startup = startup;
            context.Parameters.Url = url;
            return engine.Start(context);
        }
    }
}
