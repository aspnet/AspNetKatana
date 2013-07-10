// <copyright file="RouteTableTests.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Routing;
using Owin;
using Shouldly;
using Xunit.Extensions;

#if NET40
namespace Microsoft.Owin.Host40.IntegrationTests
#else

namespace Microsoft.Owin.Host45.IntegrationTests
#endif
{
    public class RouteTableTests : TestBase
    {
        public void SimpleOwinRoute(IAppBuilder ignored)
        {
            RouteTable.Routes.MapOwinRoute("simple", app => app.Use(context =>
            {
                return context.Response.WriteAsync("Hello world!");
            }));
        }

        public void OneSomethingThree(IAppBuilder ignored)
        {
            RouteTable.Routes.MapOwinRoute("one/{something}/three", app => app.Use(context =>
            {
                var httpContext = context.Get<System.Web.HttpContextBase>("System.Web.HttpContextBase");
                RouteValueDictionary values = httpContext.Request.RequestContext.RouteData.Values;
                return context.Response.WriteAsync("Hello, " + values["something"]);
            }));
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        public Task ItShouldMatchSimpleRoute(string serverName)
        {
            int port = RunWebServer(
                serverName,
                SimpleOwinRoute,
                "web.routetable.config");

            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            return client.GetStringAsync("http://localhost:" + port + "/simple")
                .Then(response => response.ShouldBe("Hello world!"));
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        public Task RouteUrlMayContainDataTokens(string serverName)
        {
            int port = RunWebServer(
                serverName,
                OneSomethingThree,
                "web.routetable.config");

            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            return client.GetStringAsync("http://localhost:" + port + "/one/two/three")
                .Then(response => response.ShouldBe("Hello, two"));
        }
    }
}
