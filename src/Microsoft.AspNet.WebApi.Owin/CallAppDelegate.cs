using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.WebApi.Owin.CallHeaders;
using Owin;

namespace Microsoft.AspNet.WebApi.Owin
{
    using AppDelegate = Func<IDictionary<string, object>, Task>;

    public class CallAppDelegate : HttpMessageHandler
    {
        private readonly AppDelegate _app;

        public CallAppDelegate(AppDelegate app)
        {
            _app = app;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("This can be done, but will be tricky");
            //return Utils.GetCallParameters(request)
            //    .Then(call => _app.Invoke(call)
            //    .Then(() => Utils.GetResponseMessage(call)));
        }
    }
}

