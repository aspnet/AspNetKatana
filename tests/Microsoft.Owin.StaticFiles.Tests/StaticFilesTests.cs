// -----------------------------------------------------------------------
// <copyright file="StaticFileTests.cs" company="Katana contributors">
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

    public class StaticFilesTests
    {
        [Theory]
        [InlineData("/", @"\", "/missing.file")]
        [InlineData("/subdir/", @"\", "/subdir/missing.file")]
        [InlineData("/missing.file", @"\missing.file", "/missing.file")]
        [InlineData("/", @"\missingsubdir\", "/xunit.xml")]
        public void NoMatch_PassesThrough(string baseUrl, string baseDir, string requestUrl)
        {
            IAppBuilder builder = new AppBuilder();
            builder.UseStaticFiles(baseUrl, Environment.CurrentDirectory + baseDir);
            AppFunc app = (AppFunc)builder.Build(typeof(AppFunc));

            IDictionary<string, object> env = CreateEmptyRequest(requestUrl);
            app(env).Wait();

            Assert.Equal(404, env["owin.ResponseStatusCode"]);
        }

        [Theory]
        [InlineData("/", @"\", "/xunit.xml")]
        [InlineData("/", @"\", "/Xunit.Xml")]
        [InlineData("/somedir/", @"\", "/somedir/xunit.xml")]
        [InlineData("/SomeDir/", @"\", "/soMediR/xunit.XmL")]
        [InlineData("/xunit.xml", @"\xunit.xml", "/xunit.xml")]
        [InlineData("/somedir/xunit.xml", @"\xunit.xml", "/somedir/xunit.xml")]
        public void FoundFile_Served(string baseUrl, string baseDir, string requestUrl)
        {
            IAppBuilder builder = new AppBuilder();
            builder.UseStaticFiles(baseUrl, Environment.CurrentDirectory + baseDir);
            AppFunc app = (AppFunc)builder.Build(typeof(AppFunc));

            IDictionary<string, object> env = CreateEmptyRequest(requestUrl);
            app(env).Wait();

            var responseHeaders = (IDictionary<string, string[]>)env["owin.ResponseHeaders"];
            Assert.Equal("text/xml", responseHeaders["Content-Type"][0]);
            Assert.True(responseHeaders["Content-Length"][0].Length > 0);
            Assert.Equal(responseHeaders["Content-Length"][0], ((Stream)env["owin.ResponseBody"]).Length.ToString());
        }

        [Theory]
        [InlineData("/", @"\", "/xunit.xml")]
        [InlineData("/", @"\", "/Xunit.Xml")]
        [InlineData("/somedir/", @"\", "/somedir/xunit.xml")]
        [InlineData("/SomeDir/", @"\", "/soMediR/xunit.XmL")]
        [InlineData("/xunit.xml", @"\xunit.xml", "/xunit.xml")]
        [InlineData("/somedir/xunit.xml", @"\xunit.xml", "/somedir/xunit.xml")]
        public void PostFile_PassesThrough(string baseUrl, string baseDir, string requestUrl)
        {
            IAppBuilder builder = new AppBuilder();
            builder.UseStaticFiles(baseUrl, Environment.CurrentDirectory + baseDir);
            AppFunc app = (AppFunc)builder.Build(typeof(AppFunc));

            IDictionary<string, object> env = CreateEmptyRequest(requestUrl);
            env["owin.RequestMethod"] = "POST";
            app(env).Wait();

            Assert.Equal(404, env["owin.ResponseStatusCode"]);
        }

        [Theory]
        [InlineData("/", @"\", "/xunit.xml")]
        [InlineData("/", @"\", "/Xunit.Xml")]
        [InlineData("/somedir/", @"\", "/somedir/xunit.xml")]
        [InlineData("/SomeDir/", @"\", "/soMediR/xunit.XmL")]
        [InlineData("/xunit.xml", @"\xunit.xml", "/xunit.xml")]
        [InlineData("/somedir/xunit.xml", @"\xunit.xml", "/somedir/xunit.xml")]
        public void HeadFile_HeadersButNotBodyServed(string baseUrl, string baseDir, string requestUrl)
        {
            IAppBuilder builder = new AppBuilder();
            builder.UseStaticFiles(baseUrl, Environment.CurrentDirectory + baseDir);
            AppFunc app = (AppFunc)builder.Build(typeof(AppFunc));

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
            Dictionary<string, object> env = new Dictionary<string, object>();
            env["owin.RequestPath"] = path;
            env["owin.ResponseHeaders"] = new Dictionary<string, string[]>();
            env["owin.ResponseBody"] = new MemoryStream();
            env["owin.CallCancelled"] = CancellationToken.None;
            env["owin.RequestMethod"] = "GET";

            return env;
        }
    }
}
