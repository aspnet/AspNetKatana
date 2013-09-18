// <copyright file="TestServer.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.Hosting.Engine;
using Microsoft.Owin.Hosting.Loader;
using Microsoft.Owin.Hosting.ServerFactory;
using Microsoft.Owin.Hosting.Services;
using Microsoft.Owin.Hosting.Utilities;
using Owin;

namespace Microsoft.Owin.Testing
{
    public class TestServer
    {
        private IDisposable _started;
        private Func<IDictionary<string, object>, Task> _invoke;

        public HttpMessageHandler Handler
        {
            get { return new OwinClientHandler(Invoke); }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed by caller.")]
        public HttpClient HttpClient
        {
            get { return new HttpClient(Handler) { BaseAddress = new Uri("http://localhost/") }; }
        }

        public static TestServer Create(Action<IAppBuilder> startup)
        {
            var server = new TestServer();
            server.Open(startup);
            return server;
        }

        public void Open(Action<IAppBuilder> startup)
        {
            Open(startup, null);
        }

        public void Open(Action<IAppBuilder> startup, StartOptions options)
        {
            if (startup == null)
            {
                throw new ArgumentNullException("startup");
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

        public void Close()
        {
            _started.Dispose();
            _started = null;
        }

        public Task Invoke(IDictionary<string, object> environment)
        {
            return _invoke.Invoke(environment);
        }

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
