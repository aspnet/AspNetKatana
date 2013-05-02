// <copyright file="AppFuncTransition.cs" company="Microsoft Open Technologies, Inc.">
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
using Owin;

namespace Microsoft.Owin.Infrastructure
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public sealed class AppFuncTransition : OwinMiddleware
    {
        private readonly AppFunc _next;

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public AppFuncTransition(AppFunc next) : base(null)
        {
            _next = next;
        }

        public override Task Invoke(OwinRequest request, OwinResponse response)
        {
            return _next(request.Environment);
        }

        public static void AddConversions(IAppBuilder app)
        {
            app.AddSignatureConversion<AppFunc, OwinMiddleware>(Conversion1);
            app.AddSignatureConversion<OwinMiddleware, AppFunc>(Conversion2);
        }

        private static OwinMiddleware Conversion1(AppFunc next)
        {
            return new AppFuncTransition(next);
        }

        private static AppFunc Conversion2(OwinMiddleware next)
        {
            throw new NotImplementedException();
        }
    }
}
