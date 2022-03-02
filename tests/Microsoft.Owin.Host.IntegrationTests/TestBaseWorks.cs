// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Owin;
using Shouldly;
using Xunit;

#pragma warning disable xUnit1013 // Public method should be marked as test

namespace Microsoft.Owin.Host.IntegrationTests
{
    public class TestBaseWorks : TestBase
    {
        [Fact]
        public async Task TestShouldRunWebServer()
        {
            int port = RunWebServer(
                serverName: "Microsoft.Owin.Host.SystemWeb",
                application: HelloWorld);

            var client = new HttpClient();

            var response = await client.GetAsync("http://localhost:" + port);
            var responseMessage = await response.Content.ReadAsStringAsync();
            Assert.Equal("Hello world!", responseMessage);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        public void HelloWorld(IAppBuilder app)
        {
            app.Run(context => { return context.Response.WriteAsync("Hello world!"); });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public async Task ServerMayBeSystemWebOrHttpListener(string serverName)
        {
            int port = RunWebServer(
                serverName: serverName,
                application: HelloWorld);

            var client = new HttpClient();

            var responseMessage = await client.GetStringAsync("http://localhost:" + port);
            responseMessage.ShouldBe("Hello world!");
        }
    }
}
