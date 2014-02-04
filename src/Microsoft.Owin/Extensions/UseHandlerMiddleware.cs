// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Owin.Extensions
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    /// <summary>
    /// Represents a middleware for executing in-line function middleware.
    /// </summary>
    public class UseHandlerMiddleware
    {
        private readonly AppFunc _next;
        private readonly Func<IOwinContext, Task> _handler;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.Owin.Extensions.UseHandlerMiddleware" /> class.
        /// </summary>
        /// <param name="next">The pointer to next middleware.</param>
        /// <param name="handler">A function that handles all requests.</param>
        public UseHandlerMiddleware(AppFunc next, Func<IOwinContext, Task> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }
            _next = next;
            _handler = handler;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.Owin.Extensions.UseHandlerMiddleware" /> class.
        /// </summary>
        /// <param name="next">The pointer to next middleware.</param>
        /// <param name="handler">A function that handles the request or calls the given next function.</param>
        public UseHandlerMiddleware(AppFunc next, Func<IOwinContext, Func<Task>, Task> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }
            _next = next;
            _handler = context => handler.Invoke(context, () => _next(context.Environment));
        }

        /// <summary>
        /// Invokes the handler for processing the request.
        /// </summary>
        /// <param name="environment">The OWIN context.</param>
        /// <returns>The <see cref="T:System.Threading.Tasks.Task" /> object that represents the request operation.</returns>
        public Task Invoke(IDictionary<string, object> environment)
        {
            IOwinContext context = new OwinContext(environment);
            return _handler.Invoke(context);
        }
    }
}
