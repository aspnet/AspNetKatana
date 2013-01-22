// <copyright file="DefaultFilesMiddlewareTests.cs" company="Katana contributors">
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

    public class DefaultFilesMiddlewareTests
    {
        [Theory]
        [InlineData("", @"", "/missing.dir")]
        [InlineData("/", @".", "/missing.dir/")]
        [InlineData("/subdir/", @".", "/subdir/missing.dir")]
        [InlineData("/subdir/", @"", "/subdir/missing.dir/")]
        [InlineData("/", @"\missing.subdir\", "/")]
        public void NoMatch_PassesThrough(string baseUrl, string baseDir, string requestUrl)
        {
            IAppBuilder builder = new AppBuilder();
            builder.UseDefaultFiles(baseUrl, baseDir);
            var app = (AppFunc)builder.Build(typeof(AppFunc));

            IDictionary<string, object> env = CreateEmptyRequest(requestUrl);
            app(env).Wait();

            Assert.Equal(404, env["owin.ResponseStatusCode"]);
            Assert.Equal(requestUrl, env["owin.RequestPath"]); // Should not be modified
        }

        [Theory]
        // [InlineData("", @"", "/SubFolder/")]
        [InlineData("", @".", "/SubFolder/")]
        [InlineData("/", @".", "/SubFolder/")]
        // [InlineData("/", @"\", "/SubFolder/")]
        [InlineData("", @".\", "/SubFolder/")]
        [InlineData("", @"SubFolder", "/")]
        [InlineData("", @".\SubFolder", "/")]
        [InlineData("/", @".\SubFolder", "/")]
        // [InlineData("/", @"\SubFolder", "/")]
        public void FoundDirectoryWithDefaultFile_PathModified(string baseUrl, string baseDir, string requestUrl)
        {
            IAppBuilder builder = new AppBuilder();
            builder.UseDefaultFiles(baseUrl, baseDir);
            var app = (AppFunc)builder.Build(typeof(AppFunc));

            IDictionary<string, object> env = CreateEmptyRequest(requestUrl);
            app(env).Wait();

            Assert.Equal(404, env["owin.ResponseStatusCode"]); // Passed through
            Assert.Equal(requestUrl + "default.html", env["owin.RequestPath"]); // Should be modified
        }

        [Theory]
        [InlineData("/SubFolder/", @"\", "/SubFolder/")]
        [InlineData("/SubFolder/", @"", "/somedir/")]
        [InlineData("/", @".\SubFolder", "/")]
        [InlineData("/", @".\SubFolder\", "/")]
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
    }
}
