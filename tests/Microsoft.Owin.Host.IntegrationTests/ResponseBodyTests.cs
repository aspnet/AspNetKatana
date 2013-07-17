// <copyright file="ResponseBodyTests.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

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

            app.Use(context =>
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
            app.Use(context =>
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
            return client.GetStringAsync("http://localhost:" + port).Then(result =>
            {
                Assert.Equal("Hello World", result);
            });
        }
    }
}
