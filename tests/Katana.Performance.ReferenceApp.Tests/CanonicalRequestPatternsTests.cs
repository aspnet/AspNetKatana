// <copyright file="CanonicalRequestPatternsTests.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;
using Xunit;

namespace Katana.Performance.ReferenceApp.Tests
{
    public sealed class CanonicalRequestPatternsTests : IDisposable
    {
        private readonly IDisposable _server;

        public CanonicalRequestPatternsTests()
        {
            _server = WebApplication.Start<Startup>(8080);
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
            Assert.Equal("text/html", response.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public async Task ShouldReturnSmallUrl()
        {
            var client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync("http://localhost:8080/small-immediate-syncwrite");
            Assert.Equal("text/plain", response.Content.Headers.ContentType.MediaType);
            string text = await response.Content.ReadAsStringAsync();
            Assert.Equal(1 << 10, text.Length);
        }

        [Fact]
        public async Task ShouldReturnLargeUrl()
        {
            var client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync("http://localhost:8080/large-immediate-syncwrite");
            Assert.Equal("text/plain", response.Content.Headers.ContentType.MediaType);
            string text = await response.Content.ReadAsStringAsync();
            Assert.Equal(1 << 20, text.Length);
        }
    }
}
