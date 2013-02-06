// <copyright file="KatanaEngine.cs" company="Katana contributors">
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Owin.Hosting.Builder;
using Microsoft.Owin.Hosting.Loader;
using Microsoft.Owin.Hosting.Settings;
using Microsoft.Owin.Hosting.Tracing;
using Microsoft.Owin.Hosting.Utilities;
using Owin;

namespace Microsoft.Owin.Hosting
{
    public class KatanaEngine : IKatanaEngine
    {
        private readonly IAppBuilderFactory _appBuilderFactory;
        private readonly ITraceOutputBinder _traceOutputBinder;
        private readonly IKatanaSettingsProvider _katanaSettingsProvider;
        private readonly IAppLoaderManager _appLoaderManager;

        public KatanaEngine(
            IAppBuilderFactory appBuilderFactory,
            ITraceOutputBinder traceOutputBinder,
            IKatanaSettingsProvider katanaSettingsProvider,
            IAppLoaderManager appLoaderManager)
        {
            if (appBuilderFactory == null)
            {
                throw new ArgumentNullException("appBuilderFactory");
            }
            if (traceOutputBinder == null)
            {
                throw new ArgumentNullException("traceOutputBinder");
            }
            if (katanaSettingsProvider == null)
            {
                throw new ArgumentNullException("katanaSettingsProvider");
            }
            if (appLoaderManager == null)
            {
                throw new ArgumentNullException("appLoaderManager");
            }

            _appBuilderFactory = appBuilderFactory;
            _traceOutputBinder = traceOutputBinder;
            _katanaSettingsProvider = katanaSettingsProvider;
            _appLoaderManager = appLoaderManager;
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
                IKatanaSettings settings = _katanaSettingsProvider.GetSettings();
                context.Output = _traceOutputBinder.Create(context.Parameters.OutputFile) ?? settings.DefaultOutput;
            }

            context.EnvironmentData.Add(new KeyValuePair<string, object>("host.TraceOutput", context.Output));
        }

        private void InitializeBuilder(StartContext context)
        {
            if (context.Builder == null)
            {
                context.Builder = _appBuilderFactory.Create();
            }

            if (context.Parameters.Url != null)
            {
                string scheme;
                string host;
                int port;
                string path;

                if (DeconstructUrl(context.Parameters.Url, out scheme, out host, out port, out path))
                {
                    context.Parameters.Scheme = scheme;
                    context.Parameters.Host = host;
                    context.Parameters.Port = port;
                    context.Parameters.Path = path;
                }
            }

            IKatanaSettings settings = _katanaSettingsProvider.GetSettings();
            string portString = (context.Parameters.Port ?? settings.DefaultPort ?? 8080).ToString(CultureInfo.InvariantCulture);

            var address = new Dictionary<string, object>
            {
                { "scheme", context.Parameters.Scheme ?? settings.DefaultScheme },
                { "host", context.Parameters.Host ?? settings.DefaultHost },
                { "port", portString },
                { "path", context.Parameters.Path ?? string.Empty },
            };

            context.Builder.Properties["host.Addresses"] = new List<IDictionary<string, object>> { address };
            context.Builder.Properties["host.AppName"] = context.Parameters.App;
            context.EnvironmentData.Add(new KeyValuePair<string, object>("host.AppName", context.Parameters.App));
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
            out int port,
            out string path)
        {
            url = url ?? string.Empty;

            int delimiterStart1 = url.IndexOf("://", StringComparison.Ordinal);
            if (delimiterStart1 < 0)
            {
                scheme = null;
                host = null;
                port = 0;
                path = null;
                return false;
            }
            int delimiterEnd1 = delimiterStart1 + "://".Length;

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
            if (int.TryParse(portString, out port))
            {
                host = url.Substring(delimiterEnd1, delimiterStart2 - delimiterEnd1);
            }
            else
            {
                if (string.Equals(scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))
                {
                    port = 80;
                }
                else if (string.Equals(scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
                {
                    port = 443;
                }
                else
                {
                    port = 0;
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
            // EventProviderTraceListener etwListener = new EventProviderTraceListener(etwGuid, "KatanaEtwListener", "::");
            var textListener = new TextWriterTraceListener(context.Output, "KatanaTraceListener");

            Trace.Listeners.Add(textListener);
            // Trace.Listeners.Add(etwListener);

            var source = new TraceSource("KatanaTraceSource", SourceLevels.All);
            source.Listeners.Add(textListener);
            // source.Listeners.Add(etwListener);

            context.Builder.Properties["host.TraceOutput"] = context.Output;
            context.Builder.Properties["host.TraceSource"] = source;
        }

        private static IDisposable EnableDisposing(StartContext context)
        {
            var cts = new CancellationTokenSource();
            context.Builder.Properties["host.OnAppDisposing"] = cts.Token;
            context.EnvironmentData.Add(new KeyValuePair<string, object>("host.OnAppDisposing", cts.Token));
            return new Disposable(() => cts.Cancel(false));
        }

        private void ResolveServerFactory(StartContext context)
        {
            // TODO- add service provider
            if (context.ServerFactory != null)
            {
                return;
            }

            IKatanaSettings settings = _katanaSettingsProvider.GetSettings();
            string serverName = context.Parameters.Server ?? settings.DefaultServer;

            // TODO: error message for server assembly not found
            Assembly serverAssembly = Assembly.Load(serverName);

            // TODO: error message for assembly does not have ServerFactory attribute
            context.ServerFactory = serverAssembly.GetCustomAttributes(false)
                .Cast<Attribute>()
                .Single(x => x.GetType().Name == "OwinServerFactoryAttribute");
        }

        private static void InitializeServerFactory(StartContext context)
        {
            MethodInfo initializeMethod = context.ServerFactory.GetType().GetMethod("Initialize", new[] { typeof(IAppBuilder) });
            if (initializeMethod != null)
            {
                initializeMethod.Invoke(context.ServerFactory, new object[] { context.Builder });
                return;
            }

            initializeMethod = context.ServerFactory.GetType().GetMethod("Initialize", new[] { typeof(IDictionary<string, object>) });
            if (initializeMethod != null)
            {
                initializeMethod.Invoke(context.ServerFactory, new object[] { context.Builder.Properties });
                return;
            }
        }

        private void ResolveApp(StartContext context)
        {
            context.Builder.UseType<Encapsulate>(context.EnvironmentData);

            if (context.App == null)
            {
                if (context.Startup == null)
                {
                    context.Startup = _appLoaderManager.Load(context.Parameters.App);
                }
                if (context.Startup == null)
                {
                    throw new EntryPointNotFoundException(Resources.Exception_MissingApplicationEntryPoint);
                }
                context.Startup(context.Builder);
            }
            else
            {
                context.Builder.Run(context.App);
            }

            context.App = context.Builder.Build();
        }

        private static IDisposable StartServer(StartContext context)
        {
            MethodInfo serverFactoryMethod = context.ServerFactory.GetType().GetMethod("Create");
            if (serverFactoryMethod == null)
            {
                throw new MissingMethodException("OwinServerFactoryAttribute", "Create");
            }
            ParameterInfo[] parameters = serverFactoryMethod.GetParameters();
            if (parameters.Length != 2)
            {
                throw new InvalidOperationException(Resources.Exception_ServerFactoryParameterCount);
            }
            if (parameters[1].ParameterType != typeof(IDictionary<string, object>))
            {
                throw new InvalidOperationException(Resources.Exception_ServerFactoryParameterType);
            }

            // let's see if we don't have the correct callable type for this server factory
            bool isExpectedAppType = parameters[0].ParameterType.IsInstanceOfType(context.App);
            if (!isExpectedAppType)
            {
                IAppBuilder builder = context.Builder.New();
                builder.Run(context.App);
                context.App = builder.Build(parameters[0].ParameterType);
            }

            return (IDisposable)serverFactoryMethod.Invoke(context.ServerFactory, new[] { context.App, context.Builder.Properties });
        }
    }
}
