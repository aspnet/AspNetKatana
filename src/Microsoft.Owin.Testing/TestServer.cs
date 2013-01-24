using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.Hosting.Loader;
using Microsoft.Owin.Hosting.Services;
using Microsoft.Owin.Hosting.Utilities;
using Owin;

namespace Microsoft.Owin.Testing
{
    public class TestServer
    {
        private static IDisposable _started;
        private Func<IDictionary<string, object>, Task> _invoke;

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

        public void Open(Action<IAppBuilder> startup, StartParameters parameters)
        {
            var testAppLoaderProvider = new TestAppLoaderProvider(startup);
            var testServerFactory = new TestServerFactory();

            var services = DefaultServices.Create(container => container.AddInstance<IAppLoaderProvider>(testAppLoaderProvider));
            var engine = services.GetService<IKatanaEngine>();
            var context = new StartContext
            {
                ServerFactory = testServerFactory,
                Parameters = parameters ?? new StartParameters()
            };
            _started = engine.Start(context);
            _invoke = testServerFactory.Invoke;
        }

        public void Close()
        {
            _started.Dispose();
            _started = null;
        }

        public HttpClient HttpClient
        {
            get
            {
                return new HttpClient(new OwinClientHandler(Invoke));
            }
        }

        public Task Invoke(IDictionary<string, object> env)
        {
            return _invoke.Invoke(env);
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

        public RequestBuilder Path(string path)
        {
            return new RequestBuilder(this, path);
        }
    }

    public class RequestBuilder
    {
        private readonly TestServer _server;
        private readonly HttpRequestMessage _req;

        public RequestBuilder(TestServer server, string path)
        {
            _server = server;
            _req = new HttpRequestMessage(HttpMethod.Get, "http://localhost" + path);
        }

        public RequestBuilder And(Action<HttpRequestMessage> configure)
        {
            configure(_req);
            return this;
        }

        public RequestBuilder Header(string name, string value)
        {
            if (!_req.Headers.TryAddWithoutValidation(name, value))
            {
                _req.Content.Headers.TryAddWithoutValidation(name, value);
            }
            return this;
        }

        public Task<HttpResponseMessage> SendAsync(string method)
        {
            _req.Method = new HttpMethod(method);
            return _server.HttpClient.SendAsync(_req);
        }

    }
}
