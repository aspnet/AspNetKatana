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

        [Fact]
        public async Task AllowPolicy_Served()
        {
            StaticFileOptions options = new StaticFileOptions() { AccessPolicy = new TestPolicy(allow: true, passThrough: false) };
            TestServer server = TestServer.Create(app => app.UseStaticFiles(options));
            HttpResponseMessage response = await server.WithPath("/xunit.xml").SendAsync("GET");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task PassThroughPolicy_PassedThrough()
        {
            StaticFileOptions options = new StaticFileOptions() { AccessPolicy = new TestPolicy(allow: false, passThrough: true) };
            TestServer server = TestServer.Create(app => app.UseStaticFiles(options));
            HttpResponseMessage response = await server.WithPath("/xunit.xml").SendAsync("GET");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task RejectPolicy_Rejected()
        {
            StaticFileOptions options = new StaticFileOptions() { AccessPolicy = new TestPolicy(rejectStatus: 401) };
            TestServer server = TestServer.Create(app => app.UseStaticFiles(options));
            HttpResponseMessage response = await server.WithPath("/xunit.xml").SendAsync("GET");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        private class TestPolicy : IFileAccessPolicy
        {
            private bool _allow;
            private bool _reject;
            private bool _passThrough;
            private int _rejectStatus;

            public TestPolicy(bool allow, bool passThrough)
            {
                _allow = allow;
                _passThrough = passThrough;
            }

            public TestPolicy(int rejectStatus)
            {
                _reject = true;
                _rejectStatus = rejectStatus;
            }

            public void CheckPolicy(FileAccessPolicyContext context)
            {
                if (_allow)
                {
                    context.Allow();
                }
                if (_reject)
                {
                    context.Reject(_rejectStatus);
                }
                if (_passThrough)
                {
                    context.PassThrough();
                }
            }
        }
    }
}
