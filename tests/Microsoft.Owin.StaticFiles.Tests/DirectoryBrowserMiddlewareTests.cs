// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Owin.Testing;
using Owin;
using Xunit;
using Xunit.Extensions;

namespace Microsoft.Owin.StaticFiles.Tests
{
    public class DirectoryBrowserMiddlewareTests
    {
        [Theory]
        [InlineData("", @"", "/missing.dir")]
        [InlineData("", @".", "/missing.dir/")]
        [InlineData("/subdir", @".", "/subdir/missing.dir")]
        [InlineData("/subdir", @"", "/subdir/missing.dir/")]
        [InlineData("", @"\missing.subdir\", "/")]
        public async Task NoMatch_PassesThrough(string baseUrl, string baseDir, string requestUrl)
        {
            TestServer server = TestServer.Create(app => app.UseDirectoryBrowser(baseUrl, baseDir));
            HttpResponseMessage response = await server.WithPath(requestUrl).SendAsync("GET");
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
            HttpResponseMessage response = await server.WithPath(requestUrl).SendAsync("GET");

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
            HttpResponseMessage response = await server.WithPath(requestUrl).SendAsync("GET");

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
            HttpResponseMessage response = await server.WithPath(requestUrl).SendAsync("POST");
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
            HttpResponseMessage response = await server.WithPath(requestUrl).SendAsync("HEAD");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("text/html", response.Content.Headers.ContentType.ToString());
            Assert.True(response.Content.Headers.ContentLength > 0);
            Assert.Equal(0, (await response.Content.ReadAsByteArrayAsync()).Length);
        }

        [Theory]
        [InlineData("text/plain", "text/plain")]
        [InlineData("text/html", "text/html")]
        [InlineData("application/json", "application/json")]
        [InlineData("*/*", "text/html")]
        [InlineData(null, "text/html")]
        [InlineData("", "text/html")]
        [InlineData("text/html, text/plain", "text/html")]
        [InlineData("text/plain, text/html", "text/html")]
        [InlineData("text/unknown, text/plain", "text/plain")]
        [InlineData("unknown/plain, *.*, text/plain", "text/plain")]
        [InlineData("unknown/plain, */*", "text/html")]
        // TODO: text/*, q rankings, etc.
        public async Task KnownAcceptContentType_Served(string acceptHeader, string expectedContentType)
        {
            TestServer server = TestServer.Create(app => app.UseDirectoryBrowser(string.Empty, string.Empty));
            HttpResponseMessage response = await server.WithPath("/").Header("Accept", acceptHeader).SendAsync("GET");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(expectedContentType, response.Content.Headers.ContentType.ToString());
            Assert.True(response.Content.Headers.ContentLength > 0);
            Assert.Equal(response.Content.Headers.ContentLength, (await response.Content.ReadAsByteArrayAsync()).Length);
        }

        [Theory]
        [InlineData("unknown")]
        [InlineData("unknown/*")]
        [InlineData("unknown/type")]
        [InlineData("unknown/type1, unknown/type2")]
        public async Task NoKnownAcceptContentType_406NotAcceptable(string acceptHeader)
        {
            TestServer server = TestServer.Create(app => app.UseDirectoryBrowser(string.Empty, string.Empty));
            HttpResponseMessage response = await server.WithPath("/").Header("Accept", acceptHeader).SendAsync("GET");
            Assert.Equal(HttpStatusCode.NotAcceptable, response.StatusCode);
        }
    }
}
