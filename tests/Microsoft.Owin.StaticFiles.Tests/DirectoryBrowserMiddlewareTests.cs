// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Owin.StaticFiles.DirectoryFormatters;
using Microsoft.Owin.StaticFiles.Filters;
using Microsoft.Owin.Testing;
using Owin;
using Xunit;
using Xunit.Extensions;

namespace Microsoft.Owin.StaticFiles.Tests
{
    public class DirectoryBrowserMiddlewareTests
    {
        [Fact]
        public async Task NullArguments()
        {
            Utilities.Throws<ArgumentNullException>(() => TestServer.Create(app => app.UseDirectoryBrowser(string.Empty, (string)null)));
            Utilities.Throws<ArgumentNullException>(() => TestServer.Create(app => app.UseDirectoryBrowser((DirectoryBrowserOptions)null)));
            Utilities.Throws<ArgumentException>(() => TestServer.Create(app => app.UseDirectoryBrowser(new DirectoryBrowserOptions() { FileSystem = null })));
            Utilities.Throws<ArgumentException>(() => TestServer.Create(app => app.UseDirectoryBrowser(new DirectoryBrowserOptions() { Formatter = null })));

            // PathString(null) is OK.
            TestServer server = TestServer.Create(app => app.UseDirectoryBrowser((string)null, string.Empty));
            var response = await server.HttpClient.GetAsync("/");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [InlineData("", @"", "/missing.dir")]
        [InlineData("", @".", "/missing.dir/")]
        [InlineData("/subdir", @".", "/subdir/missing.dir")]
        [InlineData("/subdir", @"", "/subdir/missing.dir/")]
        [InlineData("", @"\", "/missing.dir")]
        public async Task NoMatch_PassesThrough(string baseUrl, string baseDir, string requestUrl)
        {
            TestServer server = TestServer.Create(app => app.UseDirectoryBrowser(baseUrl, baseDir));
            HttpResponseMessage response = await server.CreateRequest(requestUrl).GetAsync();
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData("", @"", "/")]
        [InlineData("", @".", "/")]
        [InlineData("", @"", "/SubFolder/")]
        [InlineData("", @".", "/SubFolder/")]
        [InlineData("/somedir", @"", "/somedir/")]
        [InlineData("/somedir", @"\", "/somedir/")]
        [InlineData("/somedir", @".", "/somedir/subfolder/")]
        public async Task FoundDirectory_Served(string baseUrl, string baseDir, string requestUrl)
        {
            TestServer server = TestServer.Create(app => app.UseDirectoryBrowser(baseUrl, baseDir));
            HttpResponseMessage response = await server.CreateRequest(requestUrl).GetAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("text/html", response.Content.Headers.ContentType.ToString());
            Assert.True(response.Content.Headers.ContentLength > 0);
            Assert.Equal(response.Content.Headers.ContentLength, (await response.Content.ReadAsByteArrayAsync()).Length);
        }

        [Theory]
        [InlineData("", @"", "/SubFolder")]
        [InlineData("", @".", "/SubFolder")]
        [InlineData("/somedir", @"", "/somedir")]
        [InlineData("/somedir", @".", "/somedir/subfolder")]
        public async Task NearMatch_RedirectAddSlash(string baseUrl, string baseDir, string requestUrl)
        {
            TestServer server = TestServer.Create(app => app.UseDirectoryBrowser(baseUrl, baseDir));
            HttpResponseMessage response = await server.CreateRequest(requestUrl).GetAsync();

            Assert.Equal(HttpStatusCode.Moved, response.StatusCode);
            Assert.Equal(requestUrl + "/", response.Headers.Location.ToString());
            Assert.Equal(0, (await response.Content.ReadAsByteArrayAsync()).Length);
        }

        [Theory]
        [InlineData("", @"", "/")]
        [InlineData("", @".", "/")]
        [InlineData("", @"", "/SubFolder/")]
        [InlineData("", @".", "/SubFolder/")]
        [InlineData("/somedir", @"", "/somedir/")]
        [InlineData("/somedir", @".", "/somedir/subfolder/")]
        public async Task PostDirectory_PassesThrough(string baseUrl, string baseDir, string requestUrl)
        {
            TestServer server = TestServer.Create(app => app.UseDirectoryBrowser(baseUrl, baseDir));
            HttpResponseMessage response = await server.CreateRequest(requestUrl).PostAsync();
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData("", @"", "/")]
        [InlineData("", @".", "/")]
        [InlineData("", @"", "/SubFolder/")]
        [InlineData("", @".", "/SubFolder/")]
        [InlineData("/somedir", @"", "/somedir/")]
        [InlineData("/somedir", @".", "/somedir/subfolder/")]
        public async Task HeadDirectory_HeadersButNotBodyServed(string baseUrl, string baseDir, string requestUrl)
        {
            TestServer server = TestServer.Create(app => app.UseDirectoryBrowser(baseUrl, baseDir));
            HttpResponseMessage response = await server.CreateRequest(requestUrl).SendAsync("HEAD");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("text/html", response.Content.Headers.ContentType.ToString());
            Assert.True(response.Content.Headers.ContentLength == 0);
            Assert.Equal(0, (await response.Content.ReadAsByteArrayAsync()).Length);
        }

        [Fact]
        public async Task AllowFilter_Served()
        {
            DirectoryBrowserOptions options = new DirectoryBrowserOptions() { Filter = new TestFilter(allow: true, passThrough: false) };
            TestServer server = TestServer.Create(app => app.UseDirectoryBrowser(options));
            HttpResponseMessage response = await server.CreateRequest("/SubFolder/").GetAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task PassThroughFilter_PassedThrough()
        {
            DirectoryBrowserOptions options = new DirectoryBrowserOptions() { Filter = new TestFilter(allow: false, passThrough: true) };
            TestServer server = TestServer.Create(app => app.UseDirectoryBrowser(options));
            HttpResponseMessage response = await server.CreateRequest("/SubFolder/").GetAsync();
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task NullFilter_Served()
        {
            DirectoryBrowserOptions options = new DirectoryBrowserOptions() { Filter = null };
            TestServer server = TestServer.Create(app => app.UseDirectoryBrowser(options));
            HttpResponseMessage response = await server.CreateRequest("/SubFolder/").GetAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private class TestFilter : IRequestFilter
        {
            private bool _allow;
            private bool _passThrough;

            public TestFilter(bool allow, bool passThrough)
            {
                _allow = allow;
                _passThrough = passThrough;
            }

            public void ApplyFilter(RequestFilterContext context)
            {
                if (_allow)
                {
                    context.Allow();
                }
                if (_passThrough)
                {
                    context.PassThrough();
                }
            }
        }
    }
}
