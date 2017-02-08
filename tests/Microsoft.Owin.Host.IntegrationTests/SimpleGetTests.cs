// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading.Tasks;
using Owin;
using Shouldly;
using Xunit.Extensions;

namespace Microsoft.Owin.Host.IntegrationTests
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
        public async Task ResponseBodyShouldArrive(string serverName)
        {
            int port = RunWebServer(
                serverName,
                TextHtmlAlpha);

            var client = new HttpClient();
            var body = await client.GetStringAsync("http://localhost:" + port + "/text");
            body.ShouldBe("<p>alpha</p>");
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public async Task ContentTypeShouldBeSet(string serverName)
        {
            int port = RunWebServer(
                serverName,
                TextHtmlAlpha);

            var client = new HttpClient();
            var message = await client.GetAsync("http://localhost:" + port + "/text");
            message.Content.Headers.ContentType.MediaType.ShouldBe("text/html");
        }
    }
}
