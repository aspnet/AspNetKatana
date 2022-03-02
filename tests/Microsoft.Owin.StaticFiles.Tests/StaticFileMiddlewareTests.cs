// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.Testing;
using Owin;
using Xunit;
using Xunit.Extensions;

namespace Microsoft.Owin.StaticFiles.Tests
{
    public class StaticFileMiddlewareTests
    {
        [Fact]
        public async Task NullArguments()
        {
            Utilities.Throws<ArgumentNullException>(() => TestServer.Create(app => app.UseStaticFiles((StaticFileOptions)null)));
            Utilities.Throws<ArgumentException>(() => TestServer.Create(app => app.UseStaticFiles(new StaticFileOptions() { ContentTypeProvider = null })));

            // No exception, default provided
            TestServer.Create(app => app.UseStaticFiles(new StaticFileOptions() { FileSystem = null }));

            // PathString(null) is OK.
            TestServer server = TestServer.Create(app => app.UseStaticFiles((string)null));
            var response = await server.HttpClient.GetAsync("/");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public void GivenDirDoesntExist_Throw()
        {
            Assert.Throws<System.Reflection.TargetInvocationException>(() => TestServer.Create(app => app.UseStaticFiles("/ThisDirDoesntExist")));
        }

        [Theory]
        [InlineData("", @".", "/missing.file")]
        [InlineData("/subdir", @".", "/subdir/missing.file")]
        [InlineData("/missing.file", @"\", "/missing.file")]
        [InlineData("", @"\", "/xunit.xml")]
        public async Task NoMatch_PassesThrough(string baseUrl, string baseDir, string requestUrl)
        {
            TestServer server = TestServer.Create(app => app.UseStaticFiles(new StaticFileOptions()
            {
                RequestPath = new PathString(baseUrl),
                FileSystem = new PhysicalFileSystem(baseDir)
            }));
            HttpResponseMessage response = await server.CreateRequest(requestUrl).GetAsync();
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData("", @".", "/testrootfile.xml")]
        [InlineData("", @".", "/TestRootFile.Xml")]
        [InlineData("/somedir", @".", "/somedir/testrootfile.xml")]
        [InlineData("/SomeDir", @".", "/soMediR/testrootfile.Xml")]
        [InlineData("", @"SubFolder", "/extra.xml")]
        [InlineData("/somedir", @"SubFolder", "/somedir/extra.xml")]
        public async Task FoundFile_Served(string baseUrl, string baseDir, string requestUrl)
        {
            TestServer server = TestServer.Create(app => app.UseStaticFiles(new StaticFileOptions()
            {
                RequestPath = new PathString(baseUrl),
                FileSystem = new PhysicalFileSystem(baseDir)
            }));
            HttpResponseMessage response = await server.CreateRequest(requestUrl).GetAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("text/xml", response.Content.Headers.ContentType.ToString());
            Assert.True(response.Content.Headers.ContentLength > 0);
            Assert.Equal(response.Content.Headers.ContentLength, (await response.Content.ReadAsByteArrayAsync()).Length);
        }

        [Theory]
        [InlineData("", @".", "/testrootfile.xml")]
        [InlineData("", @".", "/Testrootfile.Xml")]
        [InlineData("/somedir", @".", "/somedir/testrootfile.xml")]
        [InlineData("/SomeDir", @".", "/soMediR/testrootfile.XmL")]
        [InlineData("", @"SubFolder", "/extra.xml")]
        [InlineData("/somedir", @"SubFolder", "/somedir/extra.xml")]
        public async Task PostFile_PassesThrough(string baseUrl, string baseDir, string requestUrl)
        {
            TestServer server = TestServer.Create(app => app.UseStaticFiles(new StaticFileOptions()
            {
                RequestPath = new PathString(baseUrl),
                FileSystem = new PhysicalFileSystem(baseDir)
            }));
            HttpResponseMessage response = await server.CreateRequest(requestUrl).PostAsync();
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData("", @".", "/testrootfile.xml")]
        [InlineData("", @".", "/Testrootfile.Xml")]
        [InlineData("/somedir", @".", "/somedir/testrootfile.xml")]
        [InlineData("/SomeDir", @".", "/soMediR/testrootfile.XmL")]
        [InlineData("", @"SubFolder", "/extra.xml")]
        [InlineData("/somedir", @"SubFolder", "/somedir/extra.xml")]
        public async Task HeadFile_HeadersButNotBodyServed(string baseUrl, string baseDir, string requestUrl)
        {
            TestServer server = TestServer.Create(app => app.UseStaticFiles(new StaticFileOptions()
            {
                RequestPath = new PathString(baseUrl),
                FileSystem = new PhysicalFileSystem(baseDir)
            }));
            HttpResponseMessage response = await server.CreateRequest(requestUrl).SendAsync("HEAD");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("text/xml", response.Content.Headers.ContentType.ToString());
            Assert.True(response.Content.Headers.ContentLength > 0);
            Assert.Empty(await response.Content.ReadAsByteArrayAsync());
        }
    }
}
