// <copyright file="AppBuilderExtensions.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;

namespace Owin
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    /// <summary>
    /// Extension methods for IAppBuilder.
    /// </summary>
    public static class AppBuilderUseExtensions
    {
        /// <summary>
        /// 
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
        /// 
        /// </summary>
        /// <param name="app"></param>
        /// <param name="handler">An app that handles all requests</param>
        /// <returns></returns>
        public static IAppBuilder Use(this IAppBuilder app, Func<IOwinContext, Task> handler)
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

        /// <summary>
        /// 
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
