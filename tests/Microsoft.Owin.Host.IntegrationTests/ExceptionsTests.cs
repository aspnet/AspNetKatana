// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.IO;
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
    public class ExceptionsTests : TestBase
    {
        public void UnhandledSyncException(IAppBuilder app)
        {
            app.Run(context => { throw new Exception(); });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task SyncException_StatusCode500Expected(string serverName)
        {
            int port = RunWebServer(
                serverName,
                UnhandledSyncException);

            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            return client.GetAsync("http://localhost:" + port + "/text")
                         .Then(response => response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError));
        }

        public void UnhandledAsyncException(IAppBuilder app)
        {
            app.Run(context => TaskHelpers.FromError(new Exception()));
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task AsyncException_StatusCode500Expected(string serverName)
        {
            int port = RunWebServer(
                serverName,
                UnhandledAsyncException);

            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            return client.GetAsync("http://localhost:" + port + "/text")
                         .Then(response => response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError));
        }

        public void OnSendingHeadersException(IAppBuilder app)
        {
            app.Run(context =>
            {
                context.Response.OnSendingHeaders(_ => { throw new Exception(); }, null);
                return TaskHelpers.Completed();
            });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task OnSendingHeadersException_StatusCode500Expected(string serverName)
        {
            int port = RunWebServer(
                serverName,
                OnSendingHeadersException);

            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            return client.GetAsync("http://localhost:" + port + "/text")
                         .Then(response => response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError));
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
#if !NET40
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
#endif
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task SyncExceptionAfterHeaders(string serverName)
        {
            int port = RunWebServer(
                serverName,
                SyncExceptionAfterHeadersApp);

            var client = new HttpClient();
            return client.GetAsync("http://localhost:" + port, HttpCompletionOption.ResponseHeadersRead)
                .Then(response =>
                {
                    Assert.True(response.IsSuccessStatusCode);
                    Stream body = response.Content.ReadAsStreamAsync().Result;
                    int read = body.Read(new byte[11], 0, 11);
                    Assert.Equal(11, read);
                    Assert.Throws<IOException>(() => body.Read(new byte[10], 0, 10));
                });
        }

        public void AsyncExceptionAfterHeadersApp(IAppBuilder app)
        {
            app.Run(context =>
            {
                context.Response.Write("Hello World");
                context.Response.Body.Flush();

                return TaskHelpers.FromError(new Exception("Failed after first write"));
            });
        }

        [Theory]
#if !NET40
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
#endif
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task AsyncExceptionAfterHeaders(string serverName)
        {
            int port = RunWebServer(
                serverName,
                AsyncExceptionAfterHeadersApp);

            var client = new HttpClient();
            return client.GetAsync("http://localhost:" + port, HttpCompletionOption.ResponseHeadersRead)
                .Then(response =>
                {
                    Assert.True(response.IsSuccessStatusCode);
                    Stream body = response.Content.ReadAsStreamAsync().Result;
                    int read = body.Read(new byte[11], 0, 11);
                    Assert.Equal(11, read);
                    Assert.Throws<IOException>(() =>
                    {
                        read = body.Read(new byte[10], 0, 10);
                        Assert.Equal(-1, read);
                    });
                });
        }

        public void ExceptionAfterHeadersWithContentLengthApp(IAppBuilder app)
        {
            app.Run(context =>
            {
                context.Response.ContentLength = 20;
                context.Response.Write("Hello World");
                context.Response.Body.Flush();

                return TaskHelpers.FromError(new Exception("Failed after first write"));
            });
        }

        [Theory]
#if !NET40
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
#endif
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task ExceptionAfterHeadersWithContentLength(string serverName)
        {
            int port = RunWebServer(
                serverName,
                ExceptionAfterHeadersWithContentLengthApp);

            var client = new HttpClient();
            return client.GetAsync("http://localhost:" + port, HttpCompletionOption.ResponseHeadersRead)
                .Then(response =>
                {
                    Assert.True(response.IsSuccessStatusCode);
                    Assert.Equal(20, response.Content.Headers.ContentLength.Value);
                    Stream body = response.Content.ReadAsStreamAsync().Result;
                    int read = body.Read(new byte[11], 0, 11);
                    Assert.Equal(11, read);
                    Assert.Throws<IOException>(() => body.Read(new byte[10], 0, 10));
                });
        }
    }
}
