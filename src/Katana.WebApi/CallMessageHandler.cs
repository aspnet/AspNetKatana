using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading;
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
            var task = _invoker.SendAsync(Utils.GetRequestMessage(env), Utils.GetCancellationToken(env));

            if (task.IsFaulted)
            {
                fault(task.Exception);
            }
            else if (task.IsCompleted)
            {
                Return(result, task.Result);
            }
            else
            {
                task.ContinueWith(
                    t =>
                    {
                        if (task.IsFaulted)
                        {
                            fault(task.Exception);
                        }
                        else if (task.IsCompleted)
                        {
                            Return(result, task.Result);
                        }
                    });
            }
        }

        private void Return(ResultDelegate result, HttpResponseMessage message)
        {
            var statusCode = ((int)message.StatusCode).ToString(CultureInfo.InvariantCulture);

            result.Invoke(
                statusCode + " " + message.ReasonPhrase,
                new ResponseHeadersWrapper(message),
                new HttpContentWrapper(message.Content).Send);
        }
    }
}

