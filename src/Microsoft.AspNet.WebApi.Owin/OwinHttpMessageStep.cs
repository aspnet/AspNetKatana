using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;


namespace Microsoft.AspNet.WebApi.Owin
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    internal abstract class OwinHttpMessageStep
    {
        /// <summary>
        /// Incoming call from prior OWIN middleware
        /// </summary>
        public Task Invoke(
            IDictionary<string, object> env)
        {
            return Invoke(
                env,
                OwinHttpMessageUtils.GetRequestMessage(env),
                OwinHttpMessageUtils.GetCancellationToken(env));
        }

        /// <summary>
        /// Call to process HttpRequestMessage at current step
        /// </summary>
        protected abstract Task Invoke(
            IDictionary<string, object> env,
            HttpRequestMessage requestMessage,
            CancellationToken cancellationToken);
        
        /// <summary>
        /// Present in OWIN pipeline to call HttpMessageInvoker. Invoker represents
        /// an entire HttpServer pipeline which will execute request and response to completion.
        /// The response message will either be transmitted without additional OWIN pipeline processing,
        /// or in the case of a 404 status the HttpResponseMessage is ignored and processing continues 
        /// to the _next step through CallAppFunc.Invoke which passes the original environment to the rest of
        /// the OWIN pipeline.
        /// </summary>
        public class CallHttpMessageInvoker : OwinHttpMessageStep
        {
            readonly HttpMessageInvoker _invoker;
            readonly OwinHttpMessageStep _next;

            public CallHttpMessageInvoker(OwinHttpMessageStep next, HttpMessageInvoker invoker)
            {
                _next = next;
                _invoker = invoker;
            }

            protected override Task Invoke(
                IDictionary<string, object> env, 
                HttpRequestMessage requestMessage, 
                CancellationToken cancellationToken)
            {
                return _invoker
                    .SendAsync(requestMessage, cancellationToken)
                    .Then(responseMessage =>
                    {
                        if (responseMessage.StatusCode == HttpStatusCode.NotFound)
                        {
                            return _next.Invoke(
                                env, requestMessage, cancellationToken);
                        }

                        return OwinHttpMessageUtils.SendResponseMessage(
                            env, responseMessage, cancellationToken);
                    });
            }
        }

        /// <summary>
        /// Present in OWIN pipeline following a CallHttpMessageInvoker step in order
        /// to call AppFunc with original environment. 
        /// </summary>
        public class CallAppFunc : OwinHttpMessageStep
        {
            readonly AppFunc _next;

            public CallAppFunc(AppFunc next)
            {
                _next = next;
            }

            protected override Task Invoke(
                IDictionary<string, object> env,
                HttpRequestMessage requestMessage,
                CancellationToken cancellationToken)
            {
                return _next.Invoke(env);
            }
        }
    }
}
