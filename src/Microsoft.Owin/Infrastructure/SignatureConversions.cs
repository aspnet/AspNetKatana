// <copyright file="SignatureConversions.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Threading.Tasks;
using Microsoft.Owin.Builder;
using Owin;

namespace Microsoft.Owin.Infrastructure
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    using MsAppFunc = Func<IOwinContext, Task>;

    /// <summary>
    /// Adds adapters between <typeref name="Func&lt;IDictionary&lt;string,object&gt;, Task&gt;"/> and OwinMiddleware.
    /// </summary>
    public static class SignatureConversions
    {
        /// <summary>
        /// Adds adapters between <typeref name="Func&lt;IDictionary&lt;string,object&gt;, Task&gt;"/> and OwinMiddleware.
        /// </summary>
        /// <param name="app"></param>
        public static void AddConversions(IAppBuilder app)
        {
            app.AddSignatureConversion<AppFunc, OwinMiddleware>(Conversion1);
            app.AddSignatureConversion<OwinMiddleware, AppFunc>(Conversion2);
            app.AddSignatureConversion<MsAppFunc, AppFunc>(Conversion3);
            app.AddSignatureConversion<AppFunc, MsAppFunc>(Conversion4);
            app.AddSignatureConversion<OwinMiddleware, MsAppFunc>(Conversion5);
        }

        private static OwinMiddleware Conversion1(AppFunc next)
        {
            return new AppFuncTransition(next);
        }

        private static AppFunc Conversion2(OwinMiddleware next)
        {
            return new OwinMiddlewareTransition(next).Invoke;
        }

        private static AppFunc Conversion3(MsAppFunc next)
        {
            return environment =>
            {
                return next.Invoke(new OwinContext(environment));
            };
        }

        private static MsAppFunc Conversion4(AppFunc next)
        {
            return context =>
            {
                return next(context.Environment);
            };
        }

        private static MsAppFunc Conversion5(OwinMiddleware next)
        {
            return next.Invoke;
        }
    }
}
