// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles.Filters;
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
            Utilities.Throws<ArgumentNullException>(() => TestServer.Create(app => app.UseStaticFiles((string)null)));
            Utilities.Throws<ArgumentNullException>(() => TestServer.Create(app => app.UseStaticFiles(string.Empty, (string)null)));
            Utilities.Throws<ArgumentNullException>(() => TestServer.Create(app => app.UseStaticFiles((StaticFileOptions)null)));
            Utilities.Throws<ArgumentException>(() => TestServer.Create(app => app.UseStaticFiles(new StaticFileOptions() { FileSystem = null })));
            Utilities.Throws<ArgumentException>(() => TestServer.Create(app => app.UseStaticFiles(new StaticFileOptions() { ContentTypeProvider = null })));

            // PathString(null) is OK.
            TestServer server = TestServer.Create(app => app.UseStaticFiles((string)null, string.Empty));
            var response = await server.HttpClient.GetAsync("/");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            // AccesssPolicy = null; is OK.
            server = TestServer.Create(app => app.UseStaticFiles(new StaticFileOptions() { Filter = null }));
            response = await server.HttpClient.GetAsync("/");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public void GivenDirDoesntExist_Throw()
        {
            Assert.Throws<DirectoryNotFoundException>(() => TestServer.Create(app => app.UseStaticFiles("ThisDirDoesntExist")));
        }

        [Theory]
        [InlineData("", @".", "/missing.file")]
        [InlineData("/subdir", @".", "/subdir/missing.file")]
        [InlineData("/missing.file", @"\", "/missing.file")]
        [InlineData("", @"\", "/xunit.xml")]
        public async Task NoMatch_PassesThrough(string baseUrl, string baseDir, string requestUrl)
        {
            TestServer server = TestServer.Create(app => app.UseStaticFiles(baseUrl, baseDir));
            HttpResponseMessage response = await server.CreateRequest(requestUrl).GetAsync();
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
            HttpResponseMessage response = await server.CreateRequest(requestUrl).GetAsync();

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
            HttpResponseMessage response = await server.CreateRequest(requestUrl).PostAsync();
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
            HttpResponseMessage response = await server.CreateRequest(requestUrl).SendAsync("HEAD");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("text/xml", response.Content.Headers.ContentType.ToString());
            Assert.True(response.Content.Headers.ContentLength > 0);
            Assert.Equal(0, (await response.Content.ReadAsByteArrayAsync()).Length);
        }

        [Fact]
        public async Task AllowFilter_Served()
        {
            StaticFileOptions options = new StaticFileOptions() { Filter = new TestFilter(allow: true, passThrough: false) };
            TestServer server = TestServer.Create(app => app.UseStaticFiles(options));
            HttpResponseMessage response = await server.CreateRequest("/xunit.xml").GetAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task PassThroughFilter_PassedThrough()
        {
            StaticFileOptions options = new StaticFileOptions() { Filter = new TestFilter(allow: false, passThrough: true) };
            TestServer server = TestServer.Create(app => app.UseStaticFiles(options));
            HttpResponseMessage response = await server.CreateRequest("/xunit.xml").GetAsync();
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task NullFilter_Served()
        {
            StaticFileOptions options = new StaticFileOptions() { Filter = null };
            TestServer server = TestServer.Create(app => app.UseStaticFiles(options));
            HttpResponseMessage response = await server.CreateRequest("/xunit.xml").GetAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [InlineData("/bin/file.txt")]
        [InlineData("/App_Data/file.txt")]
        [InlineData("/App_globalResources/file.txt")]
        [InlineData("/SubDir/App_LocalResources/file.txt")]
        [InlineData("/app_WebReferences/file.txt")]
        [InlineData("/App_Data/subdir/file.txt")]
        [InlineData("/App_Browsers/")]
        public void DefaultFilterHit_PassThrough(string path)
        {
            IOwinContext owinContext = new OwinContext();
            owinContext.Request.Path = new PathString(path);
            RequestFilterContext context = new RequestFilterContext(owinContext, owinContext.Request.Path);
            StaticFileOptions options = new StaticFileOptions();
            IRequestFilter defaultFilter = options.Filter;
            defaultFilter.ApplyFilter(context);
            Assert.True(context.IsPassThrough);
        }

        [Theory]
        [InlineData("/App_Data")]
        [InlineData("/App_Data_Other/")]
        public void DefaultPolicyFilter_Allowed(string path)
        {
            IOwinContext owinContext = new OwinContext();
            owinContext.Request.Path = new PathString(path);
            RequestFilterContext context = new RequestFilterContext(owinContext, owinContext.Request.Path);
            StaticFileOptions options = new StaticFileOptions();
            IRequestFilter defaultFilter = options.Filter;
            defaultFilter.ApplyFilter(context);
            Assert.True(context.IsAllowed);
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
