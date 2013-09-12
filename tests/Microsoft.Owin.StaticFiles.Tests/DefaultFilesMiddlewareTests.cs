// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.IO;
using System.Threading;
using Microsoft.Owin.Builder;
using Owin;
using Xunit;
using Xunit.Extensions;

namespace Microsoft.Owin.StaticFiles.Tests
{
    public class DefaultFilesMiddlewareTests
    {
        [Theory]
        [InlineData("", @"", "/missing.dir")]
        [InlineData("", @".", "/missing.dir/")]
        [InlineData("/subdir", @".", "/subdir/missing.dir")]
        [InlineData("/subdir", @"", "/subdir/missing.dir/")]
        [InlineData("", @"missing.subdir\", "/")]
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
    }
}
