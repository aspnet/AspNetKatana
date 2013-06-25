// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Owin;

namespace Microsoft.Owin.Security.Tests
{
    public static class TestAppBuilderExtensions
    {
        /// <summary>
        /// Used as a convenience to put a piece of code in the pipeline
        /// </summary>
        /// <param name="app"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public static IAppBuilder UseHandler(this IAppBuilder app, Func<IOwinContext, Func<Task>, Task> handler)
        {
            return app.UseFunc<Func<IDictionary<string, object>, Task>>(
                next => env => handler.Invoke(new OwinContext(env), () => next(env)));
        }
    }
}