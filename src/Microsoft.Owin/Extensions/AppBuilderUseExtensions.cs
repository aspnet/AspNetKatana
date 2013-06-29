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
        /// <param name="app">An app that handles all requests</param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static IAppBuilder UseApp(this IAppBuilder app, Func<IOwinContext, Task> func)
        {
            if (app == null)
            {
                throw new ArgumentNullException("app");
            }
            if (func == null)
            {
                throw new ArgumentNullException("func");
            }

            return app.Use(new Func<object, AppFunc>(ignored => 
                environment => func(new OwinContext(environment))));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        /// <param name="func">An app that handles the request or calls next</param>
        /// <returns></returns>
        public static IAppBuilder UseHandler(this IAppBuilder app, Func<IOwinContext, Func<Task> /* next */, Task> func)
        {
            if (app == null)
            {
                throw new ArgumentNullException("app");
            }
            if (func == null)
            {
                throw new ArgumentNullException("func");
            }

            return app.Use(new Func<AppFunc, AppFunc>(nextApp =>
            {
                return environment => 
                {
                    Func<Task> next = () => nextApp(environment);
                    return func(new OwinContext(environment), next);
                };
            }));
        }
    }
}
