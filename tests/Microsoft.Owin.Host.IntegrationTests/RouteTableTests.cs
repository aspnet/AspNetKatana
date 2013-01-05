// <copyright file="RouteTableTests.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
// </copyright>
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Routing;
using Microsoft.Owin.Host.SystemWeb;
using Owin;
using Shouldly;
using Xunit.Extensions;

#if NET40
namespace Microsoft.Owin.Host40.IntegrationTests
#else
namespace Microsoft.Owin.Host45.IntegrationTests
#endif
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class RouteTableTests : TestBase
    {
        public void SimpleOwinRoute(IAppBuilder ignored)
        {
            RouteTable.Routes.MapOwinRoute("simple", app => app.UseFunc(_ => env =>
            {
                var output = (Stream)env["owin.ResponseBody"];
                using (var writer = new StreamWriter(output))
                {
                    writer.Write("Hello world!");
                }
                return TaskHelpers.Completed();
            }));
        }

        public void OneSomethingThree(IAppBuilder ignored)
        {
            RouteTable.Routes.MapOwinRoute("one/{something}/three", app => app.UseFunc(_ => env =>
            {
                var output = (Stream)env["owin.ResponseBody"];
                var context = (System.Web.HttpContextBase)env["System.Web.HttpContextBase"];
                var values = context.Request.RequestContext.RouteData.Values;

                using (var writer = new StreamWriter(output))
                {
                    writer.Write("Hello, {0}!", values["something"]);
                }
                return TaskHelpers.Completed();
            }));
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        public Task ItShouldMatchSimpleRoute(string serverName)
        {
            var port = RunWebServer(
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
            var port = RunWebServer(
                serverName,
                OneSomethingThree,
                "web.routetable.config");

            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            return client.GetStringAsync("http://localhost:" + port + "/one/two/three")
                .Then(response => response.ShouldBe("Hello, two!"));
        }
    }
}
