// <copyright file="CanonicalRequestPatternsTests.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
// </copyright>
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

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;
using Shouldly;
using Xunit;

namespace Katana.Performance.ReferenceApp.Tests
{
    public sealed class CanonicalRequestPatternsTests : IDisposable
    {
        private readonly IDisposable _server;

        public CanonicalRequestPatternsTests()
        {
            _server = WebApplication.Start<Startup>(port: 8080);
        }

        public void Dispose()
        {
            _server.Dispose();
        }

        [Fact]
        public async Task ShouldReturnIndex()
        {
            var client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync("http://localhost:8080/");
            response.Content.Headers.ContentType.MediaType.ShouldBe("text/html");
        }

        [Fact]
        public async Task ShouldReturnSmallUrl()
        {
            var client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync("http://localhost:8080/small-immediate-syncwrite");
            response.Content.Headers.ContentType.MediaType.ShouldBe("text/plain");
            string text = await response.Content.ReadAsStringAsync();
            text.Length.ShouldBe(1 << 10);
        }

        [Fact]
        public async Task ShouldReturnLargeUrl()
        {
            var client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync("http://localhost:8080/large-immediate-syncwrite");
            response.Content.Headers.ContentType.MediaType.ShouldBe("text/plain");
            string text = await response.Content.ReadAsStringAsync();
            text.Length.ShouldBe(1 << 20);
        }
    }
}
