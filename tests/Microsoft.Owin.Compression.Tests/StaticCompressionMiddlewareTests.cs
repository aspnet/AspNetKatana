// <copyright file="StaticCompressionMiddlewareTests.cs" company="Microsoft Open Technologies, Inc.">
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

using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Owin.Testing;
using Owin;
using Shouldly;
using Xunit;

namespace Microsoft.Owin.Compression.Tests
{
    public class StaticCompressionMiddlewareTests
    {
        [Fact]
        public async Task StaticCompressionNoEffectOnSimpleRequest()
        {
            TestServer server = TestServer.Create(app => app
                .UseStaticCompression()
                .UseHandler(async (request, response, next) =>
                {
                    response.StatusCode = 200;
                    response.SetHeader("Content-Type", "text/plain");
                    await response.Body.WriteAsync(System.Text.Encoding.UTF8.GetBytes("Hello"), 0, 5);
                }));

            HttpResponseMessage resp = await server.WithPath("/hello").SendAsync("GET");

            resp.Content.Headers.ContentEncoding.ShouldBeEmpty();

            server.Close();
        }

        [Fact]
        public async Task StaticCompressionWorksWithAcceptEncodingAndETag()
        {
            TestServer server = TestServer.Create(app => app
                .UseStaticCompression()
                .UseHandler(async (request, response, next) =>
                {
                    response.StatusCode = 200;
                    response.SetHeader("Content-Type", "text/plain");
                    response.SetHeader("ETag", "\"test-etag\"");
                    await response.Body.WriteAsync(System.Text.Encoding.UTF8.GetBytes("Hello"), 0, 5);
                }));

            HttpResponseMessage resp = await server
                .WithPath("/hello")
                .And(req => req.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip")))
                .SendAsync("GET");

            resp.Content.Headers.ContentEncoding.ShouldBe(new[] { "gzip" });

            resp.Headers.ETag.Tag.ShouldBe("\"test-etag^gzip\"");

            server.Close();
        }
    }
}
