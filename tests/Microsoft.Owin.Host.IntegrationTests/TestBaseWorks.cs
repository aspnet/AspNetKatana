// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading.Tasks;
using Owin;
using Shouldly;
using Xunit;
using Xunit.Extensions;

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

            var responseMessage = await client.GetStringAsync("http://localhost:" + port);
            responseMessage.ShouldBe("Hello world!");
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
