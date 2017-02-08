// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.Owin
{
    /// <summary>
    /// An abstract base class for a standard middleware pattern.
    /// </summary>
    public abstract class OwinMiddleware
    {
        /// <summary>
        /// Instantiates the middleware with an optional pointer to the next component.
        /// </summary>
        /// <param name="next"></param>
        protected OwinMiddleware(OwinMiddleware next)
        {
            Next = next;
        }

        /// <summary>
        /// The optional next component.
        /// </summary>
        protected OwinMiddleware Next { get; set; }

        /// <summary>
        /// Process an individual request.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public abstract Task Invoke(IOwinContext context);
    }
}
