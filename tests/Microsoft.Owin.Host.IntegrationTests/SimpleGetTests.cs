// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
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
    public class SimpleGetTests : TestBase
    {
        public void TextHtmlAlpha(IAppBuilder app)
        {
            app.Run(context =>
            {
                context.Response.ContentType = "text/html";
                return context.Response.WriteAsync("<p>alpha</p>");
            });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task ResponseBodyShouldArrive(string serverName)
        {
            int port = RunWebServer(
                serverName,
                TextHtmlAlpha);

            var client = new HttpClient();
            return client.GetStringAsync("http://localhost:" + port + "/text")
                         .Then(body => body.ShouldBe("<p>alpha</p>"));
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task ContentTypeShouldBeSet(string serverName)
        {
            int port = RunWebServer(
                serverName,
                TextHtmlAlpha);

            var client = new HttpClient();
            return client.GetAsync("http://localhost:" + port + "/text")
                         .Then(message => message.Content.Headers.ContentType.MediaType.ShouldBe("text/html"));
        }
    }
}
