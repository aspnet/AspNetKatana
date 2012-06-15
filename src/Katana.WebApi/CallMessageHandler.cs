using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Katana.WebApi.CallContent;
using Katana.WebApi.CallHeaders;
using Owin;

namespace Katana.WebApi
{
    public class CallMessageHandler
    {
        private readonly HttpMessageInvoker _invoker;

        public CallMessageHandler(HttpMessageHandler handler)
        {
            _invoker = new HttpMessageInvoker(handler, disposeHandler: true);
        }

        public void Send(IDictionary<string, object> env, ResultDelegate result, Action<Exception> fault)
        {
            var requestMessage = Utils.GetRequestMessage(env);
            var cancellationToken = Utils.GetCancellationToken(env);

            _invoker
                .SendAsync(requestMessage, cancellationToken)
                .Then(responseMessage =>
                {
                    var statusCode = ((int)responseMessage.StatusCode).ToString(CultureInfo.InvariantCulture);

                    result.Invoke(
                        statusCode + " " + responseMessage.ReasonPhrase,
                        new ResponseHeadersWrapper(responseMessage),
                        new HttpContentWrapper(responseMessage.Content).Send);
                }, cancellationToken)
                .Catch(info =>
                {
                    fault(info.Exception);
                    return info.Handled();
                }, cancellationToken);
        }
    }
}

