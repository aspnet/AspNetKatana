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
        public static IAppBuilder UseHandler(this IAppBuilder app, Func<OwinRequest, OwinResponse, Func<Task>, Task> handler)
        {
            return app.UseFunc<Func<IDictionary<string, object>, Task>>(
                next => env => handler.Invoke(new OwinRequest(env), new OwinResponse(env), () => next(env))
                );
        }
    }
}