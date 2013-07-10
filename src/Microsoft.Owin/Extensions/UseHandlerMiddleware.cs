// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Microsoft.Owin.Extensions
{
    public class UseHandlerMiddleware : OwinMiddleware
    {
        private readonly Func<IOwinContext, Task> _handler;

        public UseHandlerMiddleware(OwinMiddleware next, Func<IOwinContext, Task> handler)
            : base(next)
        {
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }
            _handler = handler;
        }

        public UseHandlerMiddleware(OwinMiddleware next, Func<IOwinContext, Func<Task>, Task> handler)
            : base(next)
        {
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }
            _handler = context => handler.Invoke(context, () => Next.Invoke(context));
        }

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
