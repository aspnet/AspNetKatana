using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
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
            CallParameters call = new CallParameters();
            call.Environment = Utils.GetOwinEnvironment(request);
            // call.Headers = // TODO:
            return _app.Invoke(call).Then(result =>
            {
                return Utils.GetResponseMessage(call, result);
            });
        }
    }
}

