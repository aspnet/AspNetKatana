// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Owin.Infrastructure
{
    /// <summary>
    /// Transitions between <typeref name="Func&lt;IDictionary&lt;string,object&gt;, Task&gt;"/> and OwinMiddleware.
    /// </summary>
    internal sealed class OwinMiddlewareTransition
    {
        private readonly OwinMiddleware _next;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="next"></param>
        public OwinMiddlewareTransition(OwinMiddleware next)
        {
            _next = next;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment">OWIN environment dictionary which stores state information about the request, response and relevant server state.</param>
        /// <returns></returns>
        public Task Invoke(IDictionary<string, object> environment)
        {
            return _next.Invoke(new OwinContext(environment));
        }
    }
}
