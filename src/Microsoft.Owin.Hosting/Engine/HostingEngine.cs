// <copyright file="HostingEngine.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting.Builder;
using Microsoft.Owin.Hosting.Loader;
using Microsoft.Owin.Hosting.ServerFactory;
using Microsoft.Owin.Hosting.Tracing;
using Microsoft.Owin.Hosting.Utilities;

namespace Microsoft.Owin.Hosting.Engine
{
    public class HostingEngine : IHostingEngine
    {
        private readonly IAppBuilderFactory _appBuilderFactory;
        private readonly ITraceOutputBinder _traceOutputBinder;
        private readonly IAppLoader _appLoader;
        private readonly IServerFactoryLoader _serverFactoryLoader;

        public HostingEngine(
            IAppBuilderFactory appBuilderFactory,
            ITraceOutputBinder traceOutputBinder,
            IAppLoader appLoader,
            IServerFactoryLoader serverFactoryLoader)
        {
            if (appBuilderFactory == null)
            {
                throw new ArgumentNullException("appBuilderFactory");
            }
            if (traceOutputBinder == null)
            {
                throw new ArgumentNullException("traceOutputBinder");
            }
            if (appLoader == null)
            {
                throw new ArgumentNullException("appLoader");
            }

            _appBuilderFactory = appBuilderFactory;
            _traceOutputBinder = traceOutputBinder;
            _appLoader = appLoader;
            _serverFactoryLoader = serverFactoryLoader;
        }

        public IDisposable Start(StartContext context)
        {
            ResolveOutput(context);
            InitializeBuilder(context);
            EnableTracing(context);
            IDisposable disposablePipeline = EnableDisposing(context);
            ResolveServerFactory(context);
            InitializeServerFactory(context);
            ResolveApp(context);
            IDisposable disposableServer = StartServer(context);

            return new Disposable(
                () =>
                {
                    try
                    {
                        // first stop processing requests
                        disposableServer.Dispose();
                    }
                    finally
                    {
                        // then inform the pipeline of app shutdown
                        disposablePipeline.Dispose();
                    }
                });
        }

        private void ResolveOutput(StartContext context)
        {
            if (context.Output == null)
            {
                context.Output = _traceOutputBinder.Create(context.Options.OutputFile);
            }

            context.EnvironmentData.Add(new KeyValuePair<string, object>(Constants.HostTraceOutput, context.Output));
        }

        private void InitializeBuilder(StartContext context)
        {
            if (context.Builder == null)
            {
                context.Builder = _appBuilderFactory.Create();
            }

            var addresses = new List<IDictionary<string, object>>();

            foreach (string url in context.Options.Urls)
            {
                string scheme;
                string host;
                string port;
                string path;
                if (DeconstructUrl(url, out scheme, out host, out port, out path))
                {
                    addresses.Add(new Dictionary<string, object>
                    {
                        { Constants.Scheme, scheme },
                        { Constants.Host, host },
                        { Constants.Port, port.ToString(CultureInfo.InvariantCulture) },
                        { Constants.Path, path },
                    });
                }
            }

            if (addresses.Count == 0)
            {
                int port = DeterminePort(context);
                addresses.Add(new Dictionary<string, object>
                {
                    { Constants.Port, port.ToString(CultureInfo.InvariantCulture) },
                });
            }

            context.Builder.Properties[Constants.HostAddresses] = addresses;
            context.Builder.Properties[Constants.HostAppName] = context.Options.App;
            context.EnvironmentData.Add(new KeyValuePair<string, object>(Constants.HostAppName, context.Options.App));
        }

        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#",
            Justification = "The host may contain wildcards not supported by System.Uri")]
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#", Justification = "By design")]
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#", Justification = "By design")]
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "3#", Justification = "By design")]
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "4#", Justification = "By design")]
        public static bool DeconstructUrl(
            string url,
            out string scheme,
            out string host,
            out string port,
            out string path)
        {
            url = url ?? string.Empty;

            int delimiterStart1 = url.IndexOf(Uri.SchemeDelimiter, StringComparison.Ordinal);
            if (delimiterStart1 < 0)
            {
                scheme = null;
                host = null;
                port = null;
                path = null;
                return false;
            }
            int delimiterEnd1 = delimiterStart1 + Uri.SchemeDelimiter.Length;

            int delimiterStart3 = url.IndexOf("/", delimiterEnd1, StringComparison.Ordinal);
            if (delimiterStart3 < 0)
            {
                delimiterStart3 = url.Length;
            }
            int delimiterStart2 = url.LastIndexOf(":", delimiterStart3 - 1, delimiterStart3 - delimiterEnd1, StringComparison.Ordinal);
            int delimiterEnd2 = delimiterStart2 + ":".Length;
            if (delimiterStart2 < 0)
            {
                delimiterStart2 = delimiterStart3;
                delimiterEnd2 = delimiterStart3;
            }

            scheme = url.Substring(0, delimiterStart1);
            string portString = url.Substring(delimiterEnd2, delimiterStart3 - delimiterEnd2);
            int ignored;
            if (int.TryParse(portString, NumberStyles.Integer, CultureInfo.InvariantCulture, out ignored))
            {
                host = url.Substring(delimiterEnd1, delimiterStart2 - delimiterEnd1);
                port = portString;
            }
            else
            {
                if (string.Equals(scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))
                {
                    port = "80";
                }
                else if (string.Equals(scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
                {
                    port = "443";
                }
                else
                {
                    port = string.Empty;
                }
                host = url.Substring(delimiterEnd1, delimiterStart3 - delimiterEnd1);
            }
            path = url.Substring(delimiterStart3);
            return true;
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "Partial trust not supported")]
        private static void EnableTracing(StartContext context)
        {
            // string etwGuid = "CB50EAF9-025E-4CFB-A918-ED0F7C0CD0FA";
            // EventProviderTraceListener etwListener = new EventProviderTraceListener(etwGuid, "HostingEtwListener", "::");
            var textListener = new TextWriterTraceListener(context.Output, "HostingTraceListener");

            Trace.Listeners.Add(textListener);
            // Trace.Listeners.Add(etwListener);

            var source = new TraceSource("HostingTraceSource", SourceLevels.All);
            source.Listeners.Add(textListener);
            // source.Listeners.Add(etwListener);

            context.Builder.Properties[Constants.HostTraceOutput] = context.Output;
            context.Builder.Properties[Constants.HostTraceSource] = source;
        }

        private static IDisposable EnableDisposing(StartContext context)
        {
            var cts = new CancellationTokenSource();
            context.Builder.Properties[Constants.HostOnAppDisposing] = cts.Token;
            context.EnvironmentData.Add(new KeyValuePair<string, object>(Constants.HostOnAppDisposing, cts.Token));
            return new Disposable(() => cts.Cancel(false));
        }

        private void ResolveServerFactory(StartContext context)
        {
            if (context.ServerFactory != null)
            {
                return;
            }

            string serverName = DetermineOwinServer(context);
            context.ServerFactory = _serverFactoryLoader.Load(serverName);
            if (context.ServerFactory == null)
            {
                throw new MissingMemberException(string.Format(CultureInfo.InvariantCulture, Resources.Exception_ServerNotFound, serverName));
            }
        }

        private static string DetermineOwinServer(StartContext context)
        {
            StartOptions options = context.Options;
            IDictionary<string, string> settings = context.Options.Settings;

            string serverName = options.Server;
            if (!string.IsNullOrWhiteSpace(serverName))
            {
                return serverName;
            }

            if (settings != null &&
                settings.TryGetValue(Constants.SettingsOwinServer, out serverName) &&
                !string.IsNullOrWhiteSpace(serverName))
            {
                return serverName;
            }

            serverName = Environment.GetEnvironmentVariable(Constants.EnvOwnServer, EnvironmentVariableTarget.Process);
            if (!string.IsNullOrWhiteSpace(serverName))
            {
                return serverName;
            }

            return null;
        }

        private static int DeterminePort(StartContext context)
        {
            StartOptions options = context.Options;
            IDictionary<string, string> settings = context.Options.Settings;

            if (options != null && options.Port.HasValue)
            {
                return options.Port.Value;
            }

            string portString;
            int port;
            if (settings != null &&
                settings.TryGetValue(Constants.SettingsPort, out portString) &&
                !string.IsNullOrWhiteSpace(portString) &&
                int.TryParse(portString, NumberStyles.Integer, CultureInfo.InvariantCulture, out port))
            {
                return port;
            }

            portString = Environment.GetEnvironmentVariable(Constants.EnvPort, EnvironmentVariableTarget.Process);
            if (!string.IsNullOrWhiteSpace(portString) &&
                int.TryParse(portString, NumberStyles.Integer, CultureInfo.InvariantCulture, out port))
            {
                return port;
            }

            return Constants.DefaultPort;
        }

        private static string DetermineApplicationName(StartContext context)
        {
            StartOptions options = context.Options;
            IDictionary<string, string> settings = context.Options.Settings;

            if (options != null && !string.IsNullOrWhiteSpace(options.App))
            {
                return options.App;
            }
            
            string appName;
            if (settings.TryGetValue(Constants.SettingsOwinConfig, out appName) &&
                !string.IsNullOrWhiteSpace(appName))
            {
                return appName;
            }

            return null;
        }

        private static void InitializeServerFactory(StartContext context)
        {
            context.ServerFactory.Initialize(context.Builder);
        }

        private void ResolveApp(StartContext context)
        {
            context.Builder.Use(typeof(Encapsulate), context.EnvironmentData);

            if (context.App == null)
            {
                if (context.Startup == null)
                {
                    string appName = DetermineApplicationName(context);
                    context.Startup = _appLoader.Load(appName);
                }
                if (context.Startup == null)
                {
                    throw new EntryPointNotFoundException(Resources.Exception_MissingApplicationEntryPoint);
                }
                context.Startup(context.Builder);
            }
            else
            {
                context.Builder.Use(new Func<object, object>(_ => context.App));
            }
        }

        private static IDisposable StartServer(StartContext context)
        {
            return context.ServerFactory.Create(context.Builder);
        }
    }
}
