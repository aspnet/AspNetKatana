// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Owin;
using Xunit;
using Xunit.Extensions;

#if NET40
namespace Microsoft.Owin.Host40.IntegrationTests
#else

namespace Microsoft.Owin.Host45.IntegrationTests
#endif
{
    public class ResponseBodyTests : TestBase
    {
        public void CloseResponseBodyAndWriteExtra(IAppBuilder app)
        {
            // Delayed write
            app.Use((context, next) =>
            {
                return next()
                    .Then(() =>
                    {
                        var writer = new StreamWriter(context.Response.Body);
                        writer.Write("AndExtra");
                        writer.Flush();
                        writer.Close();
                    });
            });

            app.Run(context =>
            {
                var writer = new StreamWriter(context.Response.Body);
                writer.Write("Response");
                writer.Flush();
                writer.Close();
                return TaskHelpers.Completed();
            });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task CloseResponseBodyAndWriteExtra_CloseIgnored(string serverName)
        {
            int port = RunWebServer(
                serverName,
                CloseResponseBodyAndWriteExtra);

            var client = new HttpClient();
            return client.GetAsync("http://localhost:" + port)
                         .Then(response =>
                         {
                             response.EnsureSuccessStatusCode();
                             Assert.Equal("ResponseAndExtra", response.Content.ReadAsStringAsync().Result);
                         });
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
        public Task DisableResponseBuffering(string serverName)
        {
            int port = RunWebServer(
                serverName,
                DisableResponseBufferingApp);

            var client = new HttpClient();
            return client.GetStringAsync("http://localhost:" + port).Then(result => { Assert.Equal("Hello World", result); });
        }
    }
}
