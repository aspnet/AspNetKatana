// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Microsoft.Owin.Extensions
{
    /// <summary>
    /// Middleware for executing in-line Func middleware.
    /// </summary>
    public class UseHandlerMiddleware : OwinMiddleware
    {
        private readonly Func<IOwinContext, Task> _handler;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="next"></param>
        /// <param name="handler">An app that handles all requests</param>
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
        /// 
        /// </summary>
        /// <param name="next"></param>
        /// <param name="handler">An app that handles the request or calls the given next Func</param>
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
        /// Process an individual request.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
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
