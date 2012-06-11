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
            var tcs = new TaskCompletionSource<HttpResponseMessage>();
            var env = Utils.GetOwinEnvironment(request);
            _app.Invoke(
                env,
                (status, headers, body) =>
                {
                    try
                    {
                        tcs.SetResult(Utils.GetResponseMessage(env, status, headers, body));
                    }
                    catch (Exception ex)
                    {
                        tcs.SetException(ex);
                    }
                },
                tcs.SetException);
            return tcs.Task;
        }
    }
}

