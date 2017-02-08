// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Owin;
using Xunit;
using Xunit.Extensions;

namespace Microsoft.Owin.Host.IntegrationTests
{
    public class RequestBodyTests : TestBase
    {
        public void ReadBodyTwiceViaSeekApp(IAppBuilder app)
        {
            app.Run(context =>
            {
                var reader = new StreamReader(context.Request.Body);
                string text = reader.ReadToEnd();
                context.Response.WriteAsync(text).Wait();
                Assert.True(context.Request.Body.CanSeek);
                context.Request.Body.Seek(0, SeekOrigin.Begin);
                reader = new StreamReader(context.Request.Body);
                text = reader.ReadToEnd();
                return context.Response.WriteAsync(text);
            });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        public async Task ReadBodyTwiceViaSeek(string serverName)
        {
            int port = RunWebServer(
                serverName,
                ReadBodyTwiceViaSeekApp);

            var client = new HttpClient();
            var response = await client.PostAsync("http://localhost:" + port, new StringContent("Hello World"));
            Assert.Equal("Hello WorldHello World", await response.Content.ReadAsStringAsync());
        }

        public void ReadBodyTwiceViaPositionApp(IAppBuilder app)
        {
            app.Run(context =>
            {
                var reader = new StreamReader(context.Request.Body);
                string text = reader.ReadToEnd();
                context.Response.WriteAsync(text).Wait();
                Assert.True(context.Request.Body.CanSeek);
                context.Request.Body.Position = 0;
                reader = new StreamReader(context.Request.Body);
                text = reader.ReadToEnd();
                return context.Response.WriteAsync(text);
            });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        public async Task ReadBodyTwiceViaPosition(string serverName)
        {
            int port = RunWebServer(
                serverName,
                ReadBodyTwiceViaPositionApp);

            var client = new HttpClient();
            var response = await client.PostAsync("http://localhost:" + port, new StringContent("Hello World"));
            Assert.Equal("Hello WorldHello World", await response.Content.ReadAsStringAsync());
        }

        public void DisableRequestBufferingApp(IAppBuilder app)
        {
            app.Run(context =>
            {
                Assert.True(context.Request.Body.CanSeek);
                var disableBuffering = context.Get<Action>("server.DisableRequestBuffering");
                Assert.NotNull(disableBuffering);
                disableBuffering();
                Assert.False(context.Request.Body.CanSeek);
                var reader = new StreamReader(context.Request.Body);
                string text = reader.ReadToEnd();
                Assert.False(context.Request.Body.CanSeek);
                return context.Response.WriteAsync(text);
            });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        public async Task DisableRequestBuffering(string serverName)
        {
            int port = RunWebServer(
                serverName,
                DisableRequestBufferingApp);

            var client = new HttpClient();
            var response = await client.PostAsync("http://localhost:" + port, new StringContent("Hello World"));
            Assert.Equal("Hello World", await response.Content.ReadAsStringAsync());
        }
    }
}
