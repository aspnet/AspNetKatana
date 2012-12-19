// -----------------------------------------------------------------------
// <copyright file="DefaultFilesMiddlewareTests.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            AppFunc app = (AppFunc)builder.Build(typeof(AppFunc));

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
            AppFunc app = (AppFunc)builder.Build(typeof(AppFunc));

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
            AppFunc app = (AppFunc)builder.Build(typeof(AppFunc));

            IDictionary<string, object> env = CreateEmptyRequest(requestUrl);
            env["owin.RequestMethod"] = "POST";
            app(env).Wait();

            Assert.Equal(404, env["owin.ResponseStatusCode"]);
        }

        private IDictionary<string, object> CreateEmptyRequest(string path)
        {
            Dictionary<string, object> env = new Dictionary<string, object>();
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