// <copyright file="StaticFileMiddlewareTests.cs" company="Katana contributors">
//   Copyright 2011-2013 Katana contributors
// </copyright>
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

    public class StaticFileMiddlewareTests
    {
        [Theory]
        [InlineData("", @".", "/missing.file")]
        [InlineData("/subdir", @".", "/subdir/missing.file")]
        [InlineData("/missing.file", @"\missing.file", "/missing.file")]
        [InlineData("", @"\missingsubdir", "/xunit.xml")]
        public void NoMatch_PassesThrough(string baseUrl, string baseDir, string requestUrl)
        {
            IAppBuilder builder = new AppBuilder();
            builder.UseStaticFiles(baseUrl, baseDir);
            var app = (AppFunc)builder.Build(typeof(AppFunc));

            IDictionary<string, object> env = CreateEmptyRequest(requestUrl);
            app(env).Wait();

            Assert.Equal(404, env["owin.ResponseStatusCode"]);
        }

        [Theory]
        [InlineData("", @".", "/xunit.xml")]
        [InlineData("", @".", "/Xunit.Xml")]
        [InlineData("/somedir", @".", "/somedir/xunit.xml")]
        [InlineData("/SomeDir", @".", "/soMediR/xunit.XmL")]
        [InlineData("", @"SubFolder", "/extra.xml")]
        [InlineData("/somedir", @"SubFolder", "/somedir/extra.xml")]
        public void FoundFile_Served(string baseUrl, string baseDir, string requestUrl)
        {
            IAppBuilder builder = new AppBuilder();
            builder.UseStaticFiles(baseUrl, baseDir);
            var app = (AppFunc)builder.Build(typeof(AppFunc));

            IDictionary<string, object> env = CreateEmptyRequest(requestUrl);
            app(env).Wait();

            var responseHeaders = (IDictionary<string, string[]>)env["owin.ResponseHeaders"];
            Assert.Equal("text/xml", responseHeaders["Content-Type"][0]);
            Assert.True(responseHeaders["Content-Length"][0].Length > 0);
            Assert.Equal(responseHeaders["Content-Length"][0], ((Stream)env["owin.ResponseBody"]).Length.ToString());
        }

        [Theory]
        [InlineData("", @".", "/xunit.xml")]
        [InlineData("", @".", "/Xunit.Xml")]
        [InlineData("/somedir", @".", "/somedir/xunit.xml")]
        [InlineData("/SomeDir", @".", "/soMediR/xunit.XmL")]
        [InlineData("", @"SubFolder", "/extra.xml")]
        [InlineData("/somedir", @"SubFolder", "/somedir/extra.xml")]
        public void PostFile_PassesThrough(string baseUrl, string baseDir, string requestUrl)
        {
            IAppBuilder builder = new AppBuilder();
            builder.UseStaticFiles(baseUrl, baseDir);
            var app = (AppFunc)builder.Build(typeof(AppFunc));

            IDictionary<string, object> env = CreateEmptyRequest(requestUrl);
            env["owin.RequestMethod"] = "POST";
            app(env).Wait();

            Assert.Equal(404, env["owin.ResponseStatusCode"]);
        }

        [Theory]
        [InlineData("", @".", "/xunit.xml")]
        [InlineData("", @".", "/Xunit.Xml")]
        [InlineData("/somedir", @".", "/somedir/xunit.xml")]
        [InlineData("/SomeDir", @".", "/soMediR/xunit.XmL")]
        [InlineData("", @"SubFolder", "/extra.xml")]
        [InlineData("/somedir", @"SubFolder", "/somedir/extra.xml")]
        public void HeadFile_HeadersButNotBodyServed(string baseUrl, string baseDir, string requestUrl)
        {
            IAppBuilder builder = new AppBuilder();
            builder.UseStaticFiles(baseUrl, baseDir);
            var app = (AppFunc)builder.Build(typeof(AppFunc));

            IDictionary<string, object> env = CreateEmptyRequest(requestUrl);
            env["owin.RequestMethod"] = "HEAD";
            app(env).Wait();

            var responseHeaders = (IDictionary<string, string[]>)env["owin.ResponseHeaders"];
            Assert.Equal("text/xml", responseHeaders["Content-Type"][0]);
            Assert.True(responseHeaders["Content-Length"][0].Length > 0);
            Assert.Equal(0, ((Stream)env["owin.ResponseBody"]).Length);
        }

        private IDictionary<string, object> CreateEmptyRequest(string path)
        {
            var env = new Dictionary<string, object>();
            env["owin.RequestPath"] = path;
            env["owin.RequestHeaders"] = new Dictionary<string, string[]>();
            env["owin.ResponseHeaders"] = new Dictionary<string, string[]>();
            env["owin.ResponseBody"] = new MemoryStream();
            env["owin.CallCancelled"] = CancellationToken.None;
            env["owin.RequestMethod"] = "GET";

            return env;
        }
    }
}
