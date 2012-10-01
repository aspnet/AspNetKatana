using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Katana.Engine.Utils;
using Owin;
using Katana.Engine.Settings;
using System.Diagnostics;
using System.Diagnostics.Eventing;

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
            var disposablePipeline = EnableDisposing(context);
            ResolveServerFactory(context);
            InitializeServerFactory(context);
            ResolveApp(context);
            var disposableServer = StartServer(context);

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
            if (context.Output != null) return;

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

            var portString = (context.Parameters.Port ?? _settings.DefaultPort ?? 8080).ToString(CultureInfo.InvariantCulture);

            var address = new Dictionary<string, object>
            {
                {"scheme", context.Parameters.Scheme ?? _settings.DefaultScheme},
                {"host", context.Parameters.Host ?? _settings.DefaultHost},
                {"port", portString},
                {"path", context.Parameters.Path ?? ""},
            };

            context.Builder.Properties["host.Addresses"] = new List<IDictionary<string, object>> { address };
            context.Builder.Properties["host.AppName"] = context.Parameters.App;
        }

        private void EnableTracing(StartContext context)
        {
            string etwGuid = "CB50EAF9-025E-4CFB-A918-ED0F7C0CD0FA";
            EventProviderTraceListener etwListener = new EventProviderTraceListener(etwGuid, "KatanaEtwListener", "::");
            TextWriterTraceListener textListener = new TextWriterTraceListener(context.Output, "KatanaTraceListener");

            Trace.Listeners.Add(textListener);
            Trace.Listeners.Add(etwListener);

            TraceSource source = new TraceSource("KatanaTraceSource", SourceLevels.All);
            source.Listeners.Add(textListener);
            source.Listeners.Add(etwListener);

            context.Builder.Properties["host.TraceOutput"] = context.Output;
            context.Builder.Properties["host.TraceSource"] = source;
        }

        IDisposable EnableDisposing(StartContext context)
        {
            var cts = new CancellationTokenSource();
            context.Builder.Properties["host.OnAppDisposing"] = new Action<Action>(callback => cts.Token.Register(callback));
            return new Disposable(() => cts.Cancel(false));
        }

        private void ResolveServerFactory(StartContext context)
        {
            if (context.ServerFactory != null) return;

            var serverName = context.Parameters.Server ?? _settings.DefaultServer;

            // TODO: error message for server assembly not found
            var serverAssembly = Assembly.Load(_settings.ServerAssemblyPrefix + serverName);

            // TODO: error message for assembly does not have ServerFactory attribute
            context.ServerFactory = serverAssembly.GetCustomAttributes(false)
                .Cast<Attribute>()
                .Single(x => x.GetType().Name == "ServerFactory");
        }

        private void InitializeServerFactory(StartContext context)
        {
            var initializeMethod = context.ServerFactory.GetType().GetMethod("Initialize", new[] { typeof(IAppBuilder) });
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
            if (context.App == null)
            {
                var loader = _settings.LoaderFactory();
                var startup = loader.Load(context.Parameters.App);
                startup(context.Builder);
            }
            else
            {
                context.Builder.Run(context.App);
            }

            context.App = context.Builder.BuildNew<object>(builder => builder
                .UseType<Encapsulate>(context.Output)
                .Run(context.App));
        }

        private IDisposable StartServer(StartContext context)
        {
            var serverFactoryMethod = context.ServerFactory.GetType().GetMethod("Create");
            if (serverFactoryMethod == null)
            {
                throw new ApplicationException("ServerFactory must a single public Create method");
            }
            var parameters = serverFactoryMethod.GetParameters();
            if (parameters.Length != 2)
            {
                throw new ApplicationException("ServerFactory Create method must take two parameters");
            }
            if (parameters[1].ParameterType != typeof(IDictionary<string, object>))
            {
                throw new ApplicationException("ServerFactory Create second parameter must be of type IDictionary<string,object>");
            }

            // let's see if we don't have the correct callable type for this server factory
            var isExpectedAppType = parameters[0].ParameterType.IsInstanceOfType(context.App);
            if (!isExpectedAppType)
            {
                var builder = context.Builder.New();
                builder.Run(context.App);
                context.App = builder.Build(parameters[0].ParameterType);
            }

            return (IDisposable)serverFactoryMethod.Invoke(context.ServerFactory, new[] { context.App, context.Builder.Properties });
        }
    }
}
