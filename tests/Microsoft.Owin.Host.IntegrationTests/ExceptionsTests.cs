// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Owin;
using Shouldly;
using Xunit;
using Xunit.Extensions;

namespace Microsoft.Owin.Host.IntegrationTests
{
    public class ExceptionsTests : TestBase
    {
        public void UnhandledSyncException(IAppBuilder app)
        {
            app.Run(context => { throw new Exception(); });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public async Task SyncException_StatusCode500Expected(string serverName)
        {
            int port = RunWebServer(
                serverName,
                UnhandledSyncException);

            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            var response = await client.GetAsync("http://localhost:" + port + "/text");
            response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        }

        public void UnhandledAsyncException(IAppBuilder app)
        {
            app.Run(context =>
            {
                TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
                tcs.TrySetException(new Exception());
                return tcs.Task;
            });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public async Task AsyncException_StatusCode500Expected(string serverName)
        {
            int port = RunWebServer(
                serverName,
                UnhandledAsyncException);

            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            var response = await client.GetAsync("http://localhost:" + port + "/text");
            response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        }

        public void CancelSync(IAppBuilder app)
        {
            app.Run(context =>
            {
                TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
                tcs.TrySetCanceled();
                return tcs.Task;
            });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public async Task CancelSync_StatusCode500Expected(string serverName)
        {
            int port = RunWebServer(
                serverName,
                CancelSync);

            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            var response = await client.GetAsync("http://localhost:" + port + "/text");
            response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        }

        public void CancelAsync(IAppBuilder app)
        {
            app.Run(context =>
            {
                TaskCompletionSource<int> tcs = new TaskCompletionSource<int>();
                Task.Factory.StartNew(() =>
                {
                    tcs.TrySetCanceled();
                });
                return tcs.Task;
            });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public async Task CancelAsync_StatusCode500Expected(string serverName)
        {
            int port = RunWebServer(
                serverName,
                CancelAsync);

            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            var response = await client.GetAsync("http://localhost:" + port + "/text");
            response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        }

        public void OnSendingHeadersException(IAppBuilder app)
        {
            app.Run(context =>
            {
                context.Response.OnSendingHeaders(_ => { throw new Exception(); }, null);
                return Task.FromResult(0);
            });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public async Task OnSendingHeadersException_StatusCode500Expected(string serverName)
        {
            int port = RunWebServer(
                serverName,
                OnSendingHeadersException);

            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            var response = await client.GetAsync("http://localhost:" + port + "/text");
            response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        }

        public void SyncExceptionAfterHeadersApp(IAppBuilder app)
        {
            app.Run(context =>
            {
                context.Response.Write("Hello World");
                context.Response.Body.Flush();

                throw new Exception("Failed after first write");
            });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public async Task SyncExceptionAfterHeaders(string serverName)
        {
            int port = RunWebServer(
                serverName,
                SyncExceptionAfterHeadersApp);

            var client = new HttpClient();
            var response = await client.GetAsync("http://localhost:" + port, HttpCompletionOption.ResponseHeadersRead);
            Assert.True(response.IsSuccessStatusCode);
            Stream body = response.Content.ReadAsStreamAsync().Result;
            int read = body.Read(new byte[11], 0, 11);
            Assert.Equal(11, read);
            Assert.Throws<IOException>(() => body.Read(new byte[10], 0, 10));
        }

        public void AsyncExceptionAfterHeadersApp(IAppBuilder app)
        {
            app.Run(context =>
            {
                context.Response.Write("Hello World");
                context.Response.Body.Flush();

                TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
                tcs.TrySetException(new Exception("Failed after first write"));
                return tcs.Task;
            });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public async Task AsyncExceptionAfterHeaders(string serverName)
        {
            int port = RunWebServer(
                serverName,
                AsyncExceptionAfterHeadersApp);

            var client = new HttpClient();
            var response = await client.GetAsync("http://localhost:" + port, HttpCompletionOption.ResponseHeadersRead);
            Assert.True(response.IsSuccessStatusCode);
            Stream body = response.Content.ReadAsStreamAsync().Result;
            int read = body.Read(new byte[11], 0, 11);
            Assert.Equal(11, read);
            Assert.Throws<IOException>(() =>
            {
                read = body.Read(new byte[10], 0, 10);
                Assert.Equal(-1, read);
            });
        }

        public void ExceptionAfterHeadersWithContentLengthApp(IAppBuilder app)
        {
            app.Run(context =>
            {
                context.Response.ContentLength = 20;
                context.Response.Write("Hello World");
                context.Response.Body.Flush();

                TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
                tcs.TrySetException(new Exception("Failed after first write"));
                return tcs.Task;
            });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public async Task ExceptionAfterHeadersWithContentLength(string serverName)
        {
            int port = RunWebServer(
                serverName,
                ExceptionAfterHeadersWithContentLengthApp);

            var client = new HttpClient();
            var response = await client.GetAsync("http://localhost:" + port, HttpCompletionOption.ResponseHeadersRead);
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(20, response.Content.Headers.ContentLength.Value);
            Stream body = response.Content.ReadAsStreamAsync().Result;
            int read = body.Read(new byte[11], 0, 11);
            Assert.Equal(11, read);
            Assert.Throws<IOException>(() => body.Read(new byte[10], 0, 10));
        }
    }
}
