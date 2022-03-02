// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Routing;
using Owin;
using Shouldly;
using Xunit;

namespace Microsoft.Owin.Host.IntegrationTests
{
    public class RouteTableTests : TestBase
    {
        internal void SimpleOwinRoute(IAppBuilder ignored)
        {
            RouteTable.Routes.MapOwinRoute("simple", app => app.Run(context => { return context.Response.WriteAsync("Hello world!"); }));
        }

        internal void OneSomethingThree(IAppBuilder ignored)
        {
            RouteTable.Routes.MapOwinRoute("one/{something}/three", app => app.Run(context =>
            {
                var httpContext = context.Get<System.Web.HttpContextBase>("System.Web.HttpContextBase");
                RouteValueDictionary values = httpContext.Request.RequestContext.RouteData.Values;
                return context.Response.WriteAsync("Hello, " + values["something"]);
            }));
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        public async Task ItShouldMatchSimpleRoute(string serverName)
        {
            int port = RunWebServer(
                serverName,
                SimpleOwinRoute,
                "web.routetable.config");

            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            var response = await client.GetStringAsync("http://localhost:" + port + "/simple");
            response.ShouldBe("Hello world!");
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        public async Task RouteUrlMayContainDataTokens(string serverName)
        {
            int port = RunWebServer(
                serverName,
                OneSomethingThree,
                "web.routetable.config");

            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            var response = await client.GetStringAsync("http://localhost:" + port + "/one/two/three");
            response.ShouldBe("Hello, two");
        }
    }
}
