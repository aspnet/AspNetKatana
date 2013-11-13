// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin.Builder;
using Microsoft.Owin.StaticFiles.Filters;
using Microsoft.Owin.Testing;
using Owin;
using Xunit;
using Xunit.Extensions;

namespace Microsoft.Owin.StaticFiles.Tests
{
    public class DefaultFilesMiddlewareTests
    {
        [Fact]
        public async Task NullArguments()
        {
            Utilities.Throws<ArgumentNullException>(() => TestServer.Create(app => app.UseDefaultFiles((string)null)));
            Utilities.Throws<ArgumentNullException>(() => TestServer.Create(app => app.UseDefaultFiles(string.Empty, (string)null)));
            Utilities.Throws<ArgumentNullException>(() => TestServer.Create(app => app.UseDefaultFiles(string.Empty, string.Empty, null)));
            Utilities.Throws<ArgumentNullException>(() => TestServer.Create(app => app.UseDefaultFiles((DefaultFilesOptions)null)));
            Utilities.Throws<ArgumentException>(() => TestServer.Create(app => app.UseDefaultFiles(new DefaultFilesOptions() { FileSystem = null })));

            // PathString(null) is OK.
            TestServer server = TestServer.Create(app => app.UseDefaultFiles((string)null, string.Empty));
            var response = await server.HttpClient.GetAsync("/");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData("", @"", "/missing.dir")]
        [InlineData("", @".", "/missing.dir/")]
        [InlineData("/subdir", @".", "/subdir/missing.dir")]
        [InlineData("/subdir", @"", "/subdir/missing.dir/")]
        [InlineData("", @"\", "/missing.dir")]
        public void NoMatch_PassesThrough(string baseUrl, string baseDir, string requestUrl)
        {
            IAppBuilder builder = new AppBuilder();
            builder.UseDefaultFiles(baseUrl, baseDir);
            var app = (OwinMiddleware)builder.Build(typeof(OwinMiddleware));

            IOwinContext context = CreateEmptyRequest(requestUrl);
            app.Invoke(context).Wait();

            Assert.Equal(404, context.Response.StatusCode); // Passed through
            Assert.Equal(requestUrl, context.Request.Path.Value); // Should not be modified
        }

        [Theory]
        [InlineData("", @"", "/SubFolder/")]
        [InlineData("", @".", "/SubFolder/")]
        [InlineData("", @".\", "/SubFolder/")]
        [InlineData("", @"SubFolder", "/")]
        [InlineData("", @".\SubFolder", "/")]
        public void FoundDirectoryWithDefaultFile_PathModified(string baseUrl, string baseDir, string requestUrl)
        {
            IAppBuilder builder = new AppBuilder();
            builder.UseDefaultFiles(baseUrl, baseDir);
            var app = (OwinMiddleware)builder.Build(typeof(OwinMiddleware));

            IOwinContext context = CreateEmptyRequest(requestUrl);
            app.Invoke(context).Wait();

            Assert.Equal(404, context.Response.StatusCode); // Passed through
            Assert.Equal(requestUrl + "default.html", context.Request.Path.Value); // Should be modified
        }

        [Theory]
        [InlineData("/SubFolder", @"\", "/SubFolder/")]
        [InlineData("/SubFolder", @"", "/somedir/")]
        [InlineData("", @".\SubFolder", "/")]
        [InlineData("", @".\SubFolder\", "/")]
        public void PostDirectory_PassesThrough(string baseUrl, string baseDir, string requestUrl)
        {
            IAppBuilder builder = new AppBuilder();
            builder.UseDirectoryBrowser(baseUrl, baseDir);
            var app = (OwinMiddleware)builder.Build(typeof(OwinMiddleware));

            IOwinContext context = CreateEmptyRequest(requestUrl);
            context.Request.Method = "POST";
            app.Invoke(context).Wait();

            Assert.Equal(404, context.Response.StatusCode); // Passed through
        }

        private IOwinContext CreateEmptyRequest(string path)
        {
            IOwinContext context = new OwinContext();
            context.Request.PathBase = PathString.Empty;
            context.Request.Path = new PathString(path);
            context.Response.Body = new MemoryStream();
            context.Request.CallCancelled = CancellationToken.None;
            context.Request.Method = "GET";
            return context;
        }

        [Fact]
        public async Task AllowFilter_Served()
        {
            DefaultFilesOptions options = new DefaultFilesOptions() { Filter = new TestFilter(allow: true, passThrough: false) };
            TestServer server = TestServer.Create(app => app.UseDefaultFiles(options).UseStaticFiles());
            HttpResponseMessage response = await server.CreateRequest("/SubFolder/").GetAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task PassThroughFilter_PassedThrough()
        {
            DefaultFilesOptions options = new DefaultFilesOptions() { Filter = new TestFilter(allow: false, passThrough: true) };
            TestServer server = TestServer.Create(app => app.UseDefaultFiles(options).UseStaticFiles());
            HttpResponseMessage response = await server.CreateRequest("/SubFolder/").GetAsync();
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task NullFilter_Served()
        {
            DefaultFilesOptions options = new DefaultFilesOptions() { Filter = null };
            TestServer server = TestServer.Create(app => app.UseDefaultFiles(options).UseStaticFiles());
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
