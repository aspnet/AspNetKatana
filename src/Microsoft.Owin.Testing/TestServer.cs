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
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.Hosting.Loader;
using Microsoft.Owin.Hosting.ServerFactory;
using Microsoft.Owin.Hosting.Services;
using Microsoft.Owin.Hosting.Utilities;
using Owin;

namespace Microsoft.Owin.Testing
{
    public class TestServer
    {
        private static IDisposable _started;
        private Func<IDictionary<string, object>, Task> _invoke;

        public HttpClient HttpClient
        {
            get { return new HttpClient(new OwinClientHandler(Invoke)); }
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
            var testAppLoaderProvider = new TestAppLoaderProvider(startup);
            var testServerFactory = new TestServerFactory();

            IServiceProvider services = DefaultServices.Create(container => container.AddInstance<IAppLoaderProvider>(testAppLoaderProvider));
            var engine = services.GetService<IKatanaEngine>();
            var context = new StartContext
            {
                ServerFactory = new ServerFactoryAdapter(testServerFactory),
                Options = options ?? new StartOptions()
            };
            _started = engine.Start(context);
            _invoke = testServerFactory.Invoke;
        }

        public void Close()
        {
            _started.Dispose();
            _started = null;
        }

        public Task Invoke(IDictionary<string, object> env)
        {
            return _invoke.Invoke(env);
        }

        public RequestBuilder Path(string path)
        {
            return new RequestBuilder(this, path);
        }

        private class TestAppLoaderProvider : IAppLoaderProvider
        {
            private readonly Action<IAppBuilder> _startup;

            public TestAppLoaderProvider(Action<IAppBuilder> startup)
            {
                _startup = startup;
            }

            public Func<string, Action<IAppBuilder>> CreateAppLoader(Func<string, Action<IAppBuilder>> nextLoader)
            {
                return _ => _startup;
            }
        }

        private class TestServerFactory
        {
            protected Func<IDictionary<string, object>, Task> App { get; set; }
            protected IDictionary<string, object> Properties { get; set; }

            public IDisposable Create(Func<IDictionary<string, object>, Task> app, IDictionary<string, object> properties)
            {
                App = app;
                Properties = properties;
                return new Disposable(() => { });
            }

            public Task Invoke(IDictionary<string, object> env)
            {
                return App.Invoke(env);
            }
        }
    }
}
