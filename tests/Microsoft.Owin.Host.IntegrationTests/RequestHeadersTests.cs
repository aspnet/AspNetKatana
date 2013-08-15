// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Owin;
using Shouldly;
using Xunit;
using Xunit.Extensions;

#if NET40
namespace Microsoft.Owin.Host40.IntegrationTests
#else

namespace Microsoft.Owin.Host45.IntegrationTests
#endif
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
                return TaskHelpers.Completed();
            });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task SetCustomHeaders_Success(string serverName)
        {
            int port = RunWebServer(
                serverName,
                SetCustomRequestHeader);

            var client = new HttpClient();
            return client.GetAsync("http://localhost:" + port + "/custom")
                         .Then(response => response.StatusCode.ShouldBe((HttpStatusCode)ExpectedStatusCode));
        }

        public void SetKnownRequestHeader(IAppBuilder app)
        {
            app.Run(context =>
            {
                context.Request.Host = "custom:9090";
                context.Response.StatusCode = ExpectedStatusCode;
                return TaskHelpers.Completed();
            });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task SetKnownHeaders_Success(string serverName)
        {
            int port = RunWebServer(
                serverName,
                SetKnownRequestHeader);

            var client = new HttpClient();
            return client.GetAsync("http://localhost:" + port + "/known")
                         .Then(response => response.StatusCode.ShouldBe((HttpStatusCode)ExpectedStatusCode));
        }

        public void VerifyCaseInsensitivity(IAppBuilder app)
        {
            app.Run(context =>
            {
                context.Request.Headers["custom"] = "custom value";

                string roundTrip = context.Request.Headers["CuStom"];
                Assert.Equal("custom value", roundTrip);

                context.Response.StatusCode = ExpectedStatusCode;
                return TaskHelpers.Completed();
            });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task VerifyCaseInsensitivity_Success(string serverName)
        {
            int port = RunWebServer(
                serverName,
                VerifyCaseInsensitivity);

            var client = new HttpClient();
            return client.GetAsync("http://localhost:" + port + "/case")
                         .Then(response => response.StatusCode.ShouldBe((HttpStatusCode)ExpectedStatusCode));
        }
    }
}
