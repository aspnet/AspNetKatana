// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
        public async Task OpenInvokesApp()
        {
            TestServer server = new TestServer();
            server.Open(app =>
            {
                app.Run(context =>
                {
                    return context.Response.WriteAsync("OpenInvokesApp");
                });
            });

            string result = await server.HttpClient.GetStringAsync("/path");
            Assert.Equal("OpenInvokesApp", result);
        }

        [Fact]
        public async Task WelcomePage()
        {
            TestServer server = new TestServer();
            server.Open(app =>
            {
                // Disposes the stream.
                app.UseWelcomePage();
            });

            HttpResponseMessage result = await server.HttpClient.GetAsync("/");
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }
    }
}
