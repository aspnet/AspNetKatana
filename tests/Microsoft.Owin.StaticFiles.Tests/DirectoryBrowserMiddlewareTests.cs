// <copyright file="DirectoryBrowserMiddlewareTests.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Owin;
using Owin.Builder;
using Xunit;
using Xunit.Extensions;

namespace Microsoft.Owin.StaticFiles.Tests
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class DirectoryBrowserMiddlewareTests
    {
        [Theory]
        [InlineData("", @"", "/missing.dir")]
        [InlineData("", @".", "/missing.dir/")]
        [InlineData("/subdir", @".", "/subdir/missing.dir")]
        [InlineData("/subdir", @"", "/subdir/missing.dir/")]
        [InlineData("", @"\missing.subdir\", "/")]
        public void NoMatch_PassesThrough(string baseUrl, string baseDir, string requestUrl)
        {
            IAppBuilder builder = new AppBuilder();
            builder.UseDirectoryBrowser(baseUrl, baseDir);
            var app = (AppFunc)builder.Build(typeof(AppFunc));

            IDictionary<string, object> env = CreateEmptyRequest(requestUrl);
            app(env).Wait();

            Assert.Equal(404, env["owin.ResponseStatusCode"]);
        }

        [Theory]
        [InlineData("", @"", "/")]
        [InlineData("", @".", "/")]
        [InlineData("", @"", "/SubFolder/")]
        [InlineData("", @".", "/SubFolder/")]
        [InlineData("/somedir", @"", "/somedir/")]
        [InlineData("/somedir", @"\", "/somedir/")]
        [InlineData("/somedir", @".", "/somedir/subfolder/")]
        public void FoundDirectory_Served(string baseUrl, string baseDir, string requestUrl)
        {
            IAppBuilder builder = new AppBuilder();
            builder.UseDirectoryBrowser(baseUrl, baseDir);
            var app = (AppFunc)builder.Build(typeof(AppFunc));

            IDictionary<string, object> env = CreateEmptyRequest(requestUrl);
            app(env).Wait();

            Assert.False(env.ContainsKey("owin.ResponseStatusCode"));
            var responseHeaders = (IDictionary<string, string[]>)env["owin.ResponseHeaders"];
            Assert.Equal("text/html", responseHeaders["Content-Type"][0]);
            Assert.True(responseHeaders["Content-Length"][0].Length > 0);
            Assert.Equal(responseHeaders["Content-Length"][0], ((Stream)env["owin.ResponseBody"]).Length.ToString());
        }

        [Theory]
        [InlineData("", @"", "")]
        [InlineData("", @".", "")]
        [InlineData("", @"", "/SubFolder")]
        [InlineData("", @".", "/SubFolder")]
        [InlineData("/somedir", @"", "/somedir")]
        [InlineData("/somedir", @".", "/somedir/subfolder")]
        public void NearMatch_RedirectAddSlash(string baseUrl, string baseDir, string requestUrl)
        {
            IAppBuilder builder = new AppBuilder();
            builder.UseDirectoryBrowser(baseUrl, baseDir);
            var app = (AppFunc)builder.Build(typeof(AppFunc));

            IDictionary<string, object> env = CreateEmptyRequest(requestUrl);
            app(env).Wait();

            Assert.Equal(301, env["owin.ResponseStatusCode"]);
            var responseHeaders = (IDictionary<string, string[]>)env["owin.ResponseHeaders"];
            Assert.Equal(requestUrl + "/", responseHeaders["Location"][0]);
            Assert.Equal(0, ((Stream)env["owin.ResponseBody"]).Length);
        }

        [Theory]
        [InlineData("", @"", "/")]
        [InlineData("", @".", "/")]
        [InlineData("", @"", "/SubFolder/")]
        [InlineData("", @".", "/SubFolder/")]
        [InlineData("/somedir", @"", "/somedir/")]
        [InlineData("/somedir", @".", "/somedir/subfolder/")]
        public void PostDirectory_PassesThrough(string baseUrl, string baseDir, string requestUrl)
        {
            IAppBuilder builder = new AppBuilder();
            builder.UseDirectoryBrowser(baseUrl, baseDir);
            var app = (AppFunc)builder.Build(typeof(AppFunc));

            IDictionary<string, object> env = CreateEmptyRequest(requestUrl);
            env["owin.RequestMethod"] = "POST";
            app(env).Wait();

            Assert.Equal(404, env["owin.ResponseStatusCode"]);
        }

        [Theory]
        [InlineData("", @"", "/")]
        [InlineData("", @".", "/")]
        [InlineData("", @"", "/SubFolder/")]
        [InlineData("", @".", "/SubFolder/")]
        [InlineData("/somedir", @"", "/somedir/")]
        [InlineData("/somedir", @".", "/somedir/subfolder/")]
        public void HeadDirectory_HeadersButNotBodyServed(string baseUrl, string baseDir, string requestUrl)
        {
            IAppBuilder builder = new AppBuilder();
            builder.UseDirectoryBrowser(baseUrl, baseDir);
            var app = (AppFunc)builder.Build(typeof(AppFunc));

            IDictionary<string, object> env = CreateEmptyRequest(requestUrl);
            env["owin.RequestMethod"] = "HEAD";
            app(env).Wait();

            Assert.False(env.ContainsKey("owin.ResponseStatusCode"));
            var responseHeaders = (IDictionary<string, string[]>)env["owin.ResponseHeaders"];
            Assert.Equal("text/html", responseHeaders["Content-Type"][0]);
            Assert.True(responseHeaders["Content-Length"][0].Length > 0);
            Assert.Equal(0, ((Stream)env["owin.ResponseBody"]).Length);
        }

        [Theory]
        [InlineData(new[] { "text/plain" }, "text/plain")]
        [InlineData(new[] { "text/html" }, "text/html")]
        [InlineData(new[] { "application/json" }, "application/json")]
        [InlineData(new[] { "*/*" }, "text/html")]
        [InlineData(null, "text/html")]
        [InlineData(new string[] { }, "text/html")]
        [InlineData(new[] { "text/html, text/plain" }, "text/html")]
        [InlineData(new[] { "text/html", "text/plain" }, "text/html")]
        [InlineData(new[] { "text/plain, text/html" }, "text/html")]
        [InlineData(new[] { "text/plain", "text/html" }, "text/html")]
        [InlineData(new[] { "text/unknown, text/plain" }, "text/plain")]
        [InlineData(new[] { "unknown/plain, *.*, text/plain" }, "text/plain")]
        [InlineData(new[] { "unknown/plain", "*.*", "text/plain" }, "text/plain")]
        [InlineData(new[] { "unknown/plain", "*/*" }, "text/html")]
        // TODO: text/*, q rankings, etc.
        public void KnownAcceptContentType_Served(string[] acceptHeader, string expectedContentType)
        {
            IAppBuilder builder = new AppBuilder();
            builder.UseDirectoryBrowser(string.Empty, string.Empty);
            var app = (AppFunc)builder.Build(typeof(AppFunc));

            IDictionary<string, object> env = CreateEmptyRequest("/");
            SetAcceptHeader(env, acceptHeader);
            app(env).Wait();

            Assert.False(env.ContainsKey("owin.ResponseStatusCode"));
            var responseHeaders = (IDictionary<string, string[]>)env["owin.ResponseHeaders"];
            Assert.Equal(expectedContentType, responseHeaders["Content-Type"][0]);
            Assert.True(responseHeaders["Content-Length"][0].Length > 0);
            Assert.Equal(responseHeaders["Content-Length"][0], ((Stream)env["owin.ResponseBody"]).Length.ToString());
        }

        [Theory]
        // new object[] due to InlineData's params arg. Can't have a string[] as the only argument.
        [InlineData(new object[] { new[] { "" } })]
        [InlineData(new object[] { new[] { "unknown" } })]
        [InlineData(new object[] { new[] { "unknown/*" } })]
        [InlineData(new object[] { new[] { "unknown/type" } })]
        [InlineData(new object[] { new[] { "unknown/type1, unknown/type2" } })]
        [InlineData(new object[] { new[] { "unknown/type1", "unknown/type2" } })]
        public void NoKnownAcceptContentType_406NotAcceptable(string[] acceptHeader)
        {
            IAppBuilder builder = new AppBuilder();
            builder.UseDirectoryBrowser(string.Empty, string.Empty);
            var app = (AppFunc)builder.Build(typeof(AppFunc));

            IDictionary<string, object> env = CreateEmptyRequest("/");
            SetAcceptHeader(env, acceptHeader);
            app(env).Wait();

            Assert.Equal(406, env["owin.ResponseStatusCode"]);
        }

        private IDictionary<string, object> CreateEmptyRequest(string path)
        {
            var env = new Dictionary<string, object>();
            env["owin.RequestPathBase"] = string.Empty;
            env["owin.RequestPath"] = path;
            env["owin.RequestHeaders"] = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            env["owin.ResponseHeaders"] = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            env["owin.ResponseBody"] = new MemoryStream();
            env["owin.CallCancelled"] = CancellationToken.None;
            env["owin.RequestMethod"] = "GET";

            return env;
        }

        private void SetAcceptHeader(IDictionary<string, object> env, string[] acceptHeader)
        {
            var requestHeaders = (IDictionary<string, string[]>)env["owin.RequestHeaders"];
            requestHeaders["Accept"] = acceptHeader;
        }
    }
}
