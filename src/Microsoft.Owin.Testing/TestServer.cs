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
    public class TestServer
    {
        private IDisposable _started;
        private Func<IDictionary<string, object>, Task> _invoke;

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
        public static TestServer Create(Action<IAppBuilder> startup)
        {
            var server = new TestServer();
            server.Open(startup);
            return server;
        }

        /// <summary>
        /// Configures the OWIN pipeline.
        /// </summary>
        /// <param name="startup"></param>
        public void Open(Action<IAppBuilder> startup)
        {
            Open(startup, null);
        }

        /// <summary>
        /// Configures the OWIN pipeline.
        /// </summary>
        /// <param name="startup"></param>
        /// <param name="options"></param>
        public void Open(Action<IAppBuilder> startup, StartOptions options)
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
            var context = new StartContext(options ?? new StartOptions());
            context.ServerFactory = new ServerFactoryAdapter(testServerFactory);
            context.Startup = startup;
            _started = engine.Start(context);
            _invoke = testServerFactory.Invoke;
        }

        /// <summary>
        /// Disposes TestServer and OWIN pipeline.
        /// </summary>
        public void Close()
        {
            _started.Dispose();
            _started = null;
        }

        /// <summary>
        /// Directly invokes the OWIN pipeline with the given OWIN environment.
        /// </summary>
        /// <param name="environment"></param>
        /// <returns></returns>
        public Task Invoke(IDictionary<string, object> environment)
        {
            return _invoke.Invoke(environment);
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
