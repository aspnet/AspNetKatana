// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.Hosting.Engine;
using Microsoft.Owin.Hosting.ServerFactory;
using Microsoft.Owin.Hosting.Services;
using Owin;

namespace Microsoft.Owin.Testing
{
    /// <summary>
    /// Helps construct an in-memory OWIN pipeline and dispatch requests using HttpClient.
    /// </summary>
    public class TestServer : IDisposable
    {
        private IDisposable _started;
        private Func<IDictionary<string, object>, Task> _next;

        /// <summary>
        /// Creates a new TestServer instance.
        /// </summary>
        protected TestServer()
        {
        }

        /// <summary>
        /// The base handler that transitions to the OWIN pipeline.  Wrap this instance if you want to add intermediate handlers.
        /// </summary>
        public HttpMessageHandler Handler
        {
            get { return new OwinClientHandler(Invoke); }
        }

        /// <summary>
        /// Returns a new HttpClient wrapping the base Handler, capable of submitting requests to the OWIN pipeline.
        /// </summary>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed by caller.")]
        public HttpClient HttpClient
        {
            get { return new HttpClient(Handler) { BaseAddress = new Uri("http://localhost/") }; }
        }

        /// <summary>
        /// Create a new TestServer instance and configure the OWIN pipeline.
        /// </summary>
        /// <param name="startup"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed by caller")]
        public static TestServer Create(Action<IAppBuilder> startup)
        {
            var server = new TestServer();
            server.Configure(startup);
            return server;
        }

        /// <summary>
        /// Create a new TestServer instance and configure the OWIN pipeline.
        /// </summary>
        /// <typeparam name="TStartup"></typeparam>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed by caller")]
        public static TestServer Create<TStartup>()
        {
            var server = new TestServer();
            server.Configure<TStartup>();
            return server;
        }

        /// <summary>
        /// Configures the OWIN pipeline.
        /// </summary>
        /// <param name="startup"></param>
        protected void Configure(Action<IAppBuilder> startup)
        {
            Configure(startup, null);
        }

        /// <summary>
        /// Configures the OWIN pipeline.
        /// </summary>
        /// <typeparam name="TStartup"></typeparam>
        protected void Configure<TStartup>()
        {
            Configure<TStartup>(null);
        }

        /// <summary>
        /// Configures the OWIN pipeline.
        /// </summary>
        /// <param name="startup"></param>
        /// <param name="options"></param>
        protected void Configure(Action<IAppBuilder> startup, StartOptions options)
        {
            // Compare with WebApp.StartImplementation
            if (startup == null)
            {
                throw new ArgumentNullException("startup");
            }

            options = options ?? new StartOptions();
            if (string.IsNullOrWhiteSpace(options.AppStartup))
            {
                // Populate AppStartup for use in host.AppName
                options.AppStartup = startup.Method.ReflectedType.FullName;
            }

            var testServerFactory = new TestServerFactory();
            IServiceProvider services = ServicesFactory.Create();
            var engine = services.GetService<IHostingEngine>();
            var context = new StartContext(options);
            context.ServerFactory = new ServerFactoryAdapter(testServerFactory);
            context.Startup = startup;
            _started = engine.Start(context);
            _next = testServerFactory.Invoke;
        }

        /// <summary>
        /// Configures the OWIN pipeline.
        /// </summary>
        /// <param name="startup"></param>
        /// <param name="options"></param>
        protected void Configure<TStartup>(StartOptions options)
        {
            // Compare with WebApp.StartImplementation
            options = options ?? new StartOptions();
            options.AppStartup = typeof(TStartup).AssemblyQualifiedName;

            var testServerFactory = new TestServerFactory();
            IServiceProvider services = ServicesFactory.Create();
            var engine = services.GetService<IHostingEngine>();
            var context = new StartContext(options);
            context.ServerFactory = new ServerFactoryAdapter(testServerFactory);
            _started = engine.Start(context);
            _next = testServerFactory.Invoke;
        }

        /// <summary>
        /// Directly invokes the OWIN pipeline with the given OWIN environment.
        /// </summary>
        /// <param name="environment"></param>
        /// <returns></returns>
        public Task Invoke(IDictionary<string, object> environment)
        {
            return _next.Invoke(environment);
        }

        /// <summary>
        /// Begins constructing a request message for submission.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public RequestBuilder WithPath(string path)
        {
            return new RequestBuilder(this, path);
        }

        /// <summary>
        /// Disposes TestServer and OWIN pipeline.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes TestServer and OWIN pipeline.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            _started.Dispose();
        }

        private class TestServerFactory
        {
            private Func<IDictionary<string, object>, Task> _app;
            [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "For future use")]
            private IDictionary<string, object> _properties;

            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Invoked via reflection.")]
            public IDisposable Create(Func<IDictionary<string, object>, Task> app, IDictionary<string, object> properties)
            {
                _app = app;
                _properties = properties;
                return new Disposable();
            }

            public Task Invoke(IDictionary<string, object> env)
            {
                return _app.Invoke(env);
            }

            private class Disposable : IDisposable
            {
                public void Dispose()
                {
                }
            }
        }
    }
}
