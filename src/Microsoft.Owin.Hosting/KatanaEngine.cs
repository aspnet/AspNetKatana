// <copyright file="KatanaEngine.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Owin.Hosting.Settings;
using Microsoft.Owin.Hosting.Utilities;
using Owin;

namespace Microsoft.Owin.Hosting
{
    public class KatanaEngine : IKatanaEngine
    {
        private readonly IKatanaSettings _settings;

        public KatanaEngine(IKatanaSettings settings)
        {
            _settings = settings;
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
            if (context.Output != null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(context.Parameters.OutputFile))
            {
                context.Output = new StreamWriter(context.Parameters.OutputFile, true);
            }
            else
            {
                context.Output = _settings.DefaultOutput;
            }
        }

        private void InitializeBuilder(StartContext context)
        {
            if (context.Builder == null)
            {
                context.Builder = _settings.BuilderFactory.Invoke();
            }

            if (context.Parameters.Url != null)
            {
                string scheme;
                string host;
                int port;
                string path;

                if (DeconstructUrl(context.Parameters.Url, out scheme, out host, out port, out path))
                {
                    context.Parameters.Scheme = host;
                    context.Parameters.Host = host;
                    context.Parameters.Port = port;
                    context.Parameters.Path = path;
                }
            }

            string portString = (context.Parameters.Port ?? _settings.DefaultPort ?? 8080).ToString(CultureInfo.InvariantCulture);

            var address = new Dictionary<string, object>
            {
                { "scheme", context.Parameters.Scheme ?? _settings.DefaultScheme },
                { "host", context.Parameters.Host ?? _settings.DefaultHost },
                { "port", portString },
                { "path", context.Parameters.Path ?? string.Empty },
            };

            context.Builder.Properties["host.Addresses"] = new List<IDictionary<string, object>> { address };
            context.Builder.Properties["host.AppName"] = context.Parameters.App;
        }

        public static bool DeconstructUrl(
            string url,
            out string scheme,
            out string host,
            out int port,
            out string path)
        {
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
            var portString = url.Substring(delimiterEnd2, delimiterStart3 - delimiterEnd2);
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
            context.Builder.Properties["host.OnAppDisposing"] = new Action<Action>(callback => cts.Token.Register(callback));
            return new Disposable(() => cts.Cancel(false));
        }

        private void ResolveServerFactory(StartContext context)
        {
            if (context.ServerFactory != null)
            {
                return;
            }

            string serverName = context.Parameters.Server ?? _settings.DefaultServer;

            // TODO: error message for server assembly not found
            Assembly serverAssembly = Assembly.Load(_settings.ServerAssemblyPrefix + serverName);

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
            context.Builder.UseType<Encapsulate>(context.Output);

            if (context.App == null)
            {
                Func<string, Action<IAppBuilder>> loader = _settings.LoaderFactory();
                Action<IAppBuilder> startup = loader.Invoke(context.Parameters.App);
                if (startup != null)
                {
                    startup(context.Builder);
                }
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
                throw new InvalidOperationException("ServerFactory Create method must take two parameters");
            }
            if (parameters[1].ParameterType != typeof(IDictionary<string, object>))
            {
                throw new InvalidOperationException("ServerFactory Create second parameter must be of type IDictionary<string,object>");
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
