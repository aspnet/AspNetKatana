using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
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

    public class RequestHeadersTests : TestBase
    {
        private const int ExpectedStatusCode = 201;

        public void SetCustomRequestHeader(IAppBuilder app)
        {
            app.Run(new AppFunc(env =>
            {
                var requestHeaders = (IDictionary<string, string[]>)env["owin.RequestHeaders"];
                requestHeaders["custom"] = new string[] { "custom value" };
                env["owin.ResponseStatusCode"] = ExpectedStatusCode;
                return TaskHelpers.Completed();
            }));
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task SetCustomHeaders_Success(string serverName)
        {
            var port = RunWebServer(
                serverName,
                typeof(RequestHeadersTests).FullName + ".SetCustomRequestHeader");

            var client = new HttpClient();
            return client.GetAsync("http://localhost:" + port + "/custom")
                .Then(response => response.StatusCode.ShouldBe((HttpStatusCode)ExpectedStatusCode));
        }

        public void SetKnownRequestHeader(IAppBuilder app)
        {
            app.Run(new AppFunc(env =>
            {
                var requestHeaders = (IDictionary<string, string[]>)env["owin.RequestHeaders"];
                requestHeaders["Host"] = new string[] { "custom:9090" };
                env["owin.ResponseStatusCode"] = ExpectedStatusCode;
                return TaskHelpers.Completed();
            }));
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task SetKnownHeaders_Success(string serverName)
        {
            var port = RunWebServer(
                serverName,
                typeof(RequestHeadersTests).FullName + ".SetKnownRequestHeader");

            var client = new HttpClient();
            return client.GetAsync("http://localhost:" + port + "/known")
                .Then(response => response.StatusCode.ShouldBe((HttpStatusCode)ExpectedStatusCode));
        }

        public void VerifyCaseInsensitivity(IAppBuilder app)
        {
            app.Run(new AppFunc(env =>
            {
                var requestHeaders = (IDictionary<string, string[]>)env["owin.RequestHeaders"];
                requestHeaders["custom"] = new string[] { "custom value" };

                var roundTrip = requestHeaders["CuStom"];
                roundTrip.Length.ShouldBe(1);
                roundTrip[0].ShouldBe("custom value");

                env["owin.ResponseStatusCode"] = ExpectedStatusCode;
                return TaskHelpers.Completed();
            }));
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task VerifyCaseInsensitivity_Success(string serverName)
        {
            var port = RunWebServer(
                serverName,
                typeof(RequestHeadersTests).FullName + ".VerifyCaseInsensitivity");

            var client = new HttpClient();
            return client.GetAsync("http://localhost:" + port + "/case")
                .Then(response => response.StatusCode.ShouldBe((HttpStatusCode)ExpectedStatusCode));
        }
    }
}
