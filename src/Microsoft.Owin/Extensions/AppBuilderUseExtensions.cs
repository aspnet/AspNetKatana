// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;

namespace Owin
{
    /// <summary>
    /// Extension methods for <see cref="IAppBuilder"/>.
    /// </summary>
    public static class AppBuilderUseExtensions
    {
        /// <summary>
        /// Inserts a middleware into the OWIN pipeline.
        /// </summary>
        /// <typeparam name="T">The middleware type</typeparam>
        /// <param name="app"></param>
        /// <param name="args">Any additional arguments for the middleware constructor</param>
        /// <returns></returns>
        public static IAppBuilder Use<T>(this IAppBuilder app, params object[] args)
        {
            if (app == null)
            {
                throw new ArgumentNullException("app");
            }

            return app.Use(typeof(T), args);
        }

        /// <summary>
        /// Inserts into the OWIN pipeline a middleware which does not have a next middleware reference.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="handler">An app that handles all requests</param>
        public static void Run(this IAppBuilder app, Func<IOwinContext, Task> handler)
        {
            if (app == null)
            {
                throw new ArgumentNullException("app");
            }
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

            app.Use<UseHandlerMiddleware>(handler);
        }

        /// <summary>
        /// Inserts a middleware into the OWIN pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="handler">An app that handles the request or calls the given next Func</param>
        /// <returns></returns>
        public static IAppBuilder Use(this IAppBuilder app, Func<IOwinContext, Func<Task> /*next*/, Task> handler)
        {
            if (app == null)
            {
                throw new ArgumentNullException("app");
            }
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }

            return app.Use<UseHandlerMiddleware>(handler);
        }
    }
}
