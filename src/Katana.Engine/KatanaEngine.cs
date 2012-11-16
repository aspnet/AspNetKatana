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
using Katana.Engine.Settings;
using Katana.Engine.Utils;
using Owin;

namespace Katana.Engine
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

        private void EnableTracing(StartContext context)
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

        private void InitializeServerFactory(StartContext context)
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
                startup(context.Builder);
            }
            else
            {
                context.Builder.Run(context.App);
            }

            context.App = context.Builder.Build();
        }

        private IDisposable StartServer(StartContext context)
        {
            MethodInfo serverFactoryMethod = context.ServerFactory.GetType().GetMethod("Create");
            if (serverFactoryMethod == null)
            {
                throw new MissingMethodException("OwinServerFactoryAttribute", "Create");
            }
            ParameterInfo[] parameters = serverFactoryMethod.GetParameters();
            if (parameters.Length != 2)
            {
                throw new ApplicationException("ServerFactory Create method must take two parameters");
            }
            if (parameters[1].ParameterType != typeof(IDictionary<string, object>))
            {
                throw new ApplicationException("ServerFactory Create second parameter must be of type IDictionary<string,object>");
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
