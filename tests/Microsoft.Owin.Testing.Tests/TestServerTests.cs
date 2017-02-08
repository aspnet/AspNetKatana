// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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

        [Fact]
        public void CancelAborts()
        {
            TestServer server = TestServer.Create(app =>
            {
                app.Run(context =>
                {
                    TaskCompletionSource<int> tcs = new TaskCompletionSource<int>();
                    tcs.SetCanceled();
                    return tcs.Task;
                });
            });

            Assert.Throws<AggregateException>(() => { string result = server.HttpClient.GetStringAsync("/path").Result; });
        }

        [Fact]
        public async Task BaseAddressSetAndUsed()
        {
            TestServer server = TestServer.Create(app =>
            {
                app.Run(context =>
                {
                    return context.Response.WriteAsync(context.Request.Host.ToString());
                });
            });
            server.BaseAddress = new Uri("http://localhost2/");

            string result = await server.HttpClient.GetStringAsync("/path");
            Assert.Equal("localhost2", result);
        }
    }
}
