using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.WebApi.Owin.CallContent;
using Microsoft.AspNet.WebApi.Owin.CallHeaders;
using Owin;

namespace Microsoft.AspNet.WebApi.Owin
{
    public class CallMessageHandler
    {
        private readonly HttpMessageInvoker _invoker;

        public CallMessageHandler(HttpMessageHandler handler)
        {
            _invoker = new HttpMessageInvoker(handler, disposeHandler: true);
        }

        public Task<ResultParameters> Send(CallParameters call)
        {
            var requestMessage = Utils.GetRequestMessage(call);
            var cancellationToken = Utils.GetCancellationToken(call);
            
            return _invoker
                .SendAsync(requestMessage, cancellationToken)
                .Then(responseMessage => Utils.GetResultParameters(responseMessage), cancellationToken);
        }
    }
}

