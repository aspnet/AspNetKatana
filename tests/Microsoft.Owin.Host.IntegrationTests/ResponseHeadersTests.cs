// <copyright file="ResponseHeadersTests.cs" company="Microsoft Open Technologies, Inc.">
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

using System.Collections.Generic;
using System.Linq;
using System.Net;
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
    public class ResponseHeadersTests : TestBase
    {
        private const int ExpectedStatusCode = 201;

        private KeyValuePair<string, string>[] specialHeaders = new KeyValuePair<string, string>[]
        {
            new KeyValuePair<string, string>("Cache-Control", "no-cache"),
            new KeyValuePair<string, string>("Content-Encoding", "special"),
            new KeyValuePair<string, string>("Content-Length", "11"),
            new KeyValuePair<string, string>("Content-Type", "text/plain"),
            new KeyValuePair<string, string>("Expires", "1"),
            new KeyValuePair<string, string>("Location", "/"),
        };

        public void SetCustomResponseHeader(IAppBuilder app)
        {
            app.UseApp(context =>
            {
                context.Response.Headers["custom"] = "custom value";
                context.Response.StatusCode = ExpectedStatusCode;
                return TaskHelpers.Completed();
            });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task SetCustomHeaders_Success(string serverName)
        {
            int port = RunWebServer(
                serverName,
                SetCustomResponseHeader);

            var client = new HttpClient();
            return client.GetAsync("http://localhost:" + port + "/custom")
                .Then(response => 
                { 
                    Assert.Equal((HttpStatusCode)ExpectedStatusCode, response.StatusCode);
                    Assert.Equal("custom value", response.Headers.GetValues("custom").First());
                });
        }

        public void SetSpecialResponseHeaders(IAppBuilder app)
        {
            app.UseApp(context =>
            {
                var responseHeaders = context.Response.Headers;
                foreach (var header in specialHeaders)
                {
                    responseHeaders[header.Key] = header.Value;
                }

                context.Response.StatusCode = ExpectedStatusCode;
                
                // Some header issues are only visible after calling write and flush.
                context.Response.Write("Hello World");
                context.Response.Body.Flush();
                return TaskHelpers.Completed();
            });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task SetSpecialHeaders_Success(string serverName)
        {
            int port = RunWebServer(
                serverName,
                SetSpecialResponseHeaders);

            var client = new HttpClient();
            return client.GetAsync("http://localhost:" + port + "/special")
                .Then(response =>
                {
                    Assert.Equal((HttpStatusCode)ExpectedStatusCode, response.StatusCode);

                    foreach (var header in specialHeaders)
                    {
                        IEnumerable<string> values;
                        bool exists = response.Headers.TryGetValues(header.Key, out values)
                            || response.Content.Headers.TryGetValues(header.Key, out values);
                        Assert.True(exists);
                        Assert.Equal(header.Value, values.First());
                    }
                });
        }
    }
}
