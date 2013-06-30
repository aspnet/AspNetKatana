// <copyright file="RoslynAppLoaderTests.cs" company="Microsoft Open Technologies, Inc.">
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
using Microsoft.Owin;
using Microsoft.Owin.Builder;
using Owin;
using Shouldly;
using Xunit;

namespace Katana.Loader.Roslyn.Tests
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class RoslynAppLoaderTests
    {
        [Fact]
        public async Task ScriptFileCanBeExecutedByName()
        {
            var factory = new RoslynAppLoaderFactory();
            Func<string, IList<string>, Action<IAppBuilder>> loader = factory.Create((_, __) => null);
            Action<IAppBuilder> startup = loader.Invoke("Simple.csx", null);
            var builder = new AppBuilder();
            startup.Invoke(builder);
            var app = builder.Build<OwinMiddleware>();

            IOwinContext context = new OwinContext();
            await app.Invoke(context);
            context.Response.StatusCode.ShouldBe(24601);
        }
    }
}
