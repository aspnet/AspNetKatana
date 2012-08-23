using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Microsoft.AspNet.WebApi.Owin
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class HttpServerHandler
    {
        readonly AppFunc _app;
        readonly HttpMessageInvoker _invoker;

        public HttpServerHandler(AppFunc app, HttpRouteCollection routes)
        {
            _app = app;
            _invoker = new HttpMessageInvoker(new HttpServer(new HttpConfiguration(routes)));
        }

        public HttpServerHandler(AppFunc app, HttpConfiguration configuration)
        {
            _app = app;
            _invoker = new HttpMessageInvoker(new HttpServer(configuration));
        }

        public HttpServerHandler(AppFunc app, HttpMessageHandler server)
        {
            _app = app;
            _invoker = new HttpMessageInvoker(server);
        }

        public Task Invoke(IDictionary<string, object> env)
        {
            var requestMessage = Utils.GetRequestMessage(env);
            var cancellationToken = Utils.GetCancellationToken(env);
            return _invoker.SendAsync(requestMessage, cancellationToken).Then(responseMessage =>
            {
                if (responseMessage.StatusCode == HttpStatusCode.NotFound)
                {
                    return _app.Invoke(env);
                }

                return Utils.SendResponseMessage(env, responseMessage, cancellationToken);
            });
        }
    }
}
