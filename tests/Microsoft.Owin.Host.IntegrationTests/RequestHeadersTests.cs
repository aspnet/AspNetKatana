// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Owin;
using Shouldly;
using Xunit;
using Xunit.Extensions;

namespace Microsoft.Owin.Host.IntegrationTests
{
    public class RequestHeadersTests : TestBase
    {
        private const int ExpectedStatusCode = 201;

        public void SetCustomRequestHeader(IAppBuilder app)
        {
            app.Run(context =>
            {
                context.Request.Headers["custom"] = "custom value";
                context.Response.StatusCode = ExpectedStatusCode;
                return Task.FromResult(0);
            });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public async Task SetCustomHeaders_Success(string serverName)
        {
            int port = RunWebServer(
                serverName,
                SetCustomRequestHeader);

            var client = new HttpClient();
            var response = await client.GetAsync("http://localhost:" + port + "/custom");
            response.StatusCode.ShouldBe((HttpStatusCode)ExpectedStatusCode);
        }

        public void SetKnownRequestHeader(IAppBuilder app)
        {
            app.Run(context =>
            {
                context.Request.Host = new HostString("custom:9090");
                context.Response.StatusCode = ExpectedStatusCode;
                return Task.FromResult(0);
            });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public async Task SetKnownHeaders_Success(string serverName)
        {
            int port = RunWebServer(
                serverName,
                SetKnownRequestHeader);

            var client = new HttpClient();
            var response = await client.GetAsync("http://localhost:" + port + "/known");
            response.StatusCode.ShouldBe((HttpStatusCode)ExpectedStatusCode);
        }

        public void VerifyCaseInsensitivity(IAppBuilder app)
        {
            app.Run(context =>
            {
                context.Request.Headers["custom"] = "custom value";

                string roundTrip = context.Request.Headers["CuStom"];
                Assert.Equal("custom value", roundTrip);

                context.Response.StatusCode = ExpectedStatusCode;
                return Task.FromResult(0);
            });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public async Task VerifyCaseInsensitivity_Success(string serverName)
        {
            int port = RunWebServer(
                serverName,
                VerifyCaseInsensitivity);

            var client = new HttpClient();
            var response = await client.GetAsync("http://localhost:" + port + "/case");
            response.StatusCode.ShouldBe((HttpStatusCode)ExpectedStatusCode);
        }
    }
}
