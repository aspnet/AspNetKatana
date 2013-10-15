// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Owin;
using Xunit;

namespace Microsoft.Owin.Testing.Tests
{
    public class TestServerTests
    {
        [Fact]
        public async Task CreateInvokesApp()
        {
            TestServer server = TestServer.Create(app =>
            {
                app.Run(context =>
                {
                    return context.Response.WriteAsync("CreateInvokesApp");
                });
            });

            string result = await server.HttpClient.GetStringAsync("/path");
            Assert.Equal("CreateInvokesApp", result);
        }

        [Fact]
        public async Task CreateTInvokesApp()
        {
            TestServer server = TestServer.Create<Startup>();

            string result = await server.HttpClient.GetStringAsync("/path");
            Assert.Equal("Startup.Configration", result);
        }

        [Fact]
        public async Task WelcomePage()
        {
            TestServer server = TestServer.Create(app =>
            {
                // Disposes the stream.
                app.UseWelcomePage();
            });

            HttpResponseMessage result = await server.HttpClient.GetAsync("/");
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public async Task Disposed()
        {
            TestServer server = TestServer.Create(app =>
            {
                // Disposes the stream.
                app.UseWelcomePage();
            });

            HttpResponseMessage result = await server.HttpClient.GetAsync("/");
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            server.Dispose();
            AggregateException ex = Assert.Throws<AggregateException>(() => server.HttpClient.GetAsync("/").Result);
            Assert.IsType<ObjectDisposedException>(ex.GetBaseException());
        }
    }
}
