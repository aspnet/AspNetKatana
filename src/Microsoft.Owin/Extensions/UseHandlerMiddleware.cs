// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Microsoft.Owin.Extensions
{
    /// <summary>
    /// Represents a middleware for executing in-line function middleware.
    /// </summary>
    public class UseHandlerMiddleware : OwinMiddleware
    {
        private readonly Func<IOwinContext, Task> _handler;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.Owin.Extensions.UseHandlerMiddleware" /> class.
        /// </summary>
        /// <param name="next">The pointer to next middleware.</param>
        /// <param name="handler">A function that handles all requests.</param>
        public UseHandlerMiddleware(OwinMiddleware next, Func<IOwinContext, Task> handler)
            : base(next)
        {
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }
            _handler = handler;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.Owin.Extensions.UseHandlerMiddleware" /> class.
        /// </summary>
        /// <param name="next">The pointer to next middleware.</param>
        /// <param name="handler">A function that handles the request or calls the given next function.</param>
        public UseHandlerMiddleware(OwinMiddleware next, Func<IOwinContext, Func<Task>, Task> handler)
            : base(next)
        {
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }
            _handler = context => handler.Invoke(context, () => Next.Invoke(context));
        }

        /// <summary>
        /// Invokes the handler for processing the request.
        /// </summary>
        /// <param name="context">The OWIN context.</param>
        /// <returns>The <see cref="T:System.Threading.Tasks.Task" /> object that represents the request operation.</returns>
        public override Task Invoke(IOwinContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            return _handler.Invoke(context);
        }
    }
}
