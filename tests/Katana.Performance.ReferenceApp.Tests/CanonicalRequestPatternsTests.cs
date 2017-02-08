// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
            _server = WebApp.Start<Startup>("http://localhost:8080/");
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
