using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.AspNet.WebApi.Owin
{
    public class CallMessageHandler
    {
        private readonly HttpMessageInvoker _invoker;

        public CallMessageHandler(HttpMessageHandler handler)
        {
            _invoker = new HttpMessageInvoker(handler, disposeHandler: true);
        }

        public Task Invoke(IDictionary<string,object> env)
        {
            var requestMessage = Utils.GetRequestMessage(env);
            var cancellationToken = Utils.GetCancellationToken(env);
            
            return _invoker
                .SendAsync(requestMessage, cancellationToken)
                .Then(responseMessage => Utils.SendResponseMessage(env, responseMessage, cancellationToken), cancellationToken);
        }
    }
}

