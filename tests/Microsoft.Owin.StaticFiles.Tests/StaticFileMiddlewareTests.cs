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
    public class StaticFileMiddlewareTests
    {
        [Theory]
        [InlineData("", @".", "/missing.file")]
        [InlineData("/subdir", @".", "/subdir/missing.file")]
        [InlineData("/missing.file", @"\missing.file", "/missing.file")]
        [InlineData("", @"\missingsubdir", "/xunit.xml")]
        public async Task NoMatch_PassesThrough(string baseUrl, string baseDir, string requestUrl)
        {
            TestServer server = TestServer.Create(app => app.UseStaticFiles(baseUrl, baseDir));
            HttpResponseMessage response = await server.WithPath(requestUrl).SendAsync("GET");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData("", @".", "/xunit.xml")]
        [InlineData("", @".", "/Xunit.Xml")]
        [InlineData("/somedir", @".", "/somedir/xunit.xml")]
        [InlineData("/SomeDir", @".", "/soMediR/xunit.XmL")]
        [InlineData("", @"SubFolder", "/extra.xml")]
        [InlineData("/somedir", @"SubFolder", "/somedir/extra.xml")]
        public async Task FoundFile_Served(string baseUrl, string baseDir, string requestUrl)
        {
            TestServer server = TestServer.Create(app => app.UseStaticFiles(baseUrl, baseDir));
            HttpResponseMessage response = await server.WithPath(requestUrl).SendAsync("GET");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("text/xml", response.Content.Headers.ContentType.ToString());
            Assert.True(response.Content.Headers.ContentLength > 0);
            Assert.Equal(response.Content.Headers.ContentLength, (await response.Content.ReadAsByteArrayAsync()).Length);
        }

        [Theory]
        [InlineData("", @".", "/xunit.xml")]
        [InlineData("", @".", "/Xunit.Xml")]
        [InlineData("/somedir", @".", "/somedir/xunit.xml")]
        [InlineData("/SomeDir", @".", "/soMediR/xunit.XmL")]
        [InlineData("", @"SubFolder", "/extra.xml")]
        [InlineData("/somedir", @"SubFolder", "/somedir/extra.xml")]
        public async Task PostFile_PassesThrough(string baseUrl, string baseDir, string requestUrl)
        {
            TestServer server = TestServer.Create(app => app.UseStaticFiles(baseUrl, baseDir));
            HttpResponseMessage response = await server.WithPath(requestUrl).SendAsync("POST");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData("", @".", "/xunit.xml")]
        [InlineData("", @".", "/Xunit.Xml")]
        [InlineData("/somedir", @".", "/somedir/xunit.xml")]
        [InlineData("/SomeDir", @".", "/soMediR/xunit.XmL")]
        [InlineData("", @"SubFolder", "/extra.xml")]
        [InlineData("/somedir", @"SubFolder", "/somedir/extra.xml")]
        public async Task HeadFile_HeadersButNotBodyServed(string baseUrl, string baseDir, string requestUrl)
        {
            TestServer server = TestServer.Create(app => app.UseStaticFiles(baseUrl, baseDir));
            HttpResponseMessage response = await server.WithPath(requestUrl).SendAsync("HEAD");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("text/xml", response.Content.Headers.ContentType.ToString());
            Assert.True(response.Content.Headers.ContentLength > 0);
            Assert.Equal(0, (await response.Content.ReadAsByteArrayAsync()).Length);
        }
    }
}
