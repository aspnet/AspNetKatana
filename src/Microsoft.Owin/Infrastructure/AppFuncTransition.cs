// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Owin.Infrastructure
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    /// <summary>
    /// Converts between an OwinMiddlware and an <typeref name="Func&lt;IDictionary&lt;string,object&gt;, Task&gt;"/>.
    /// </summary>
    internal sealed class AppFuncTransition : OwinMiddleware
    {
        private readonly AppFunc _next;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="next"></param>
        public AppFuncTransition(AppFunc next) : base(null)
        {
            _next = next;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task Invoke(IOwinContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            return _next(context.Environment);
        }
    }
}
