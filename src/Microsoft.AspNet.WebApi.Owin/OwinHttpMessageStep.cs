// Copyright 2011-2012 Katana contributors
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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
            private readonly HttpMessageInvoker _invoker;
            private readonly OwinHttpMessageStep _next;

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
            private readonly AppFunc _next;

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
