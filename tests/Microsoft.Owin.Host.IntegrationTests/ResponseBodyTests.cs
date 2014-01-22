// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Owin;
using Xunit;
using Xunit.Extensions;

namespace Microsoft.Owin.Host.IntegrationTests
{
    public class ResponseBodyTests : TestBase
    {
        public void CloseResponseBodyAndWriteExtra(IAppBuilder app)
        {
            // Delayed write
            app.Use(async (context, next) =>
            {
                await next();
                var writer = new StreamWriter(context.Response.Body);
                writer.Write("AndExtra");
                writer.Flush();
                writer.Close();
            });

            app.Run(context =>
            {
                var writer = new StreamWriter(context.Response.Body);
                writer.Write("Response");
                writer.Flush();
                writer.Close();
                return Task.FromResult(0);
            });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public async Task CloseResponseBodyAndWriteExtra_CloseIgnored(string serverName)
        {
            int port = RunWebServer(
                serverName,
                CloseResponseBodyAndWriteExtra);

            var client = new HttpClient();
            var response = await client.GetAsync("http://localhost:" + port);
            response.EnsureSuccessStatusCode();
            Assert.Equal("ResponseAndExtra", await response.Content.ReadAsStringAsync());
        }

        public void DisableResponseBufferingApp(IAppBuilder app)
        {
            app.Run(context =>
            {
                context.Get<Action>("server.DisableResponseBuffering")();
                return context.Response.WriteAsync("Hello World");
            });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        public async Task DisableResponseBuffering(string serverName)
        {
            int port = RunWebServer(
                serverName,
                DisableResponseBufferingApp);

            var client = new HttpClient();
            var result = await client.GetStringAsync("http://localhost:" + port);
            Assert.Equal("Hello World", result);
        }
    }
}
