using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Katana.WebApi.CallHeaders;
using Owin;

namespace Katana.WebApi
{
    public class CallAppDelegate : HttpMessageHandler
    {
        private readonly AppDelegate _app;

        public CallAppDelegate(AppDelegate app)
        {
            _app = app;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Utils.GetCallParameters(request)
                .Then(call => _app.Invoke(call)
                .Then(result => Utils.GetResponseMessage(call, result)));
        }
    }
}

