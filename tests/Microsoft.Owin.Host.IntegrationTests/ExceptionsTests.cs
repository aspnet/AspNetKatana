// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Net;
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
    public class ExceptionsTests : TestBase
    {
        public void UnhandledSyncException(IAppBuilder app)
        {
            app.Run(context => { throw new Exception(); });
        }

        public void UnhandledAsyncException(IAppBuilder app)
        {
            app.Run(context => TaskHelpers.FromError(new Exception()));
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
    }
}
