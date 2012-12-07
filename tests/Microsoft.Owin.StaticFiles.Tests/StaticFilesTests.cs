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

namespace Microsoft.Owin.StaticFiles.Tests
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class StaticFilesTests
    {
        [Fact]
        public void NoPathMatch_PassesThrough()
        {
            IAppBuilder builder = new AppBuilder();
            builder.UseStaticFiles("/nomatch/", Environment.CurrentDirectory + @"\");
            AppFunc app = (AppFunc)builder.Build(typeof(AppFunc));

            IDictionary<string, object> env = CreateEmptyRequest("/somepath/somefile.txt");
            app(env).Wait();

            Assert.Equal(404, env["owin.ResponseStatusCode"]);
        }

        [Fact]
        public void PathMatchButNoFile_PassesThrough()
        {
            IAppBuilder builder = new AppBuilder();
            builder.UseStaticFiles("/somepath/", Environment.CurrentDirectory + @"\");
            AppFunc app = (AppFunc)builder.Build(typeof(AppFunc));

            IDictionary<string, object> env = CreateEmptyRequest("/somepath/somefile.txt");
            app(env).Wait();

            Assert.Equal(404, env["owin.ResponseStatusCode"]);
        }

        [Fact]
        public void FoundFile_Served()
        {
            IAppBuilder builder = new AppBuilder();
            builder.UseStaticFiles("/", Environment.CurrentDirectory + @"\");
            AppFunc app = (AppFunc)builder.Build(typeof(AppFunc));

            IDictionary<string, object> env = CreateEmptyRequest("/xunit.xml");
            app(env).Wait();

            var responseHeaders = (IDictionary<string, string[]>)env["owin.ResponseHeaders"];
            Assert.Equal("text/xml", responseHeaders["Content-Type"][0]);
            Assert.True(responseHeaders["Content-Length"][0].Length > 0);
        }

        private IDictionary<string, object> CreateEmptyRequest(string path)
        {
            Dictionary<string, object> env = new Dictionary<string, object>();
            env["owin.RequestPath"] = path;
            env["owin.ResponseHeaders"] = new Dictionary<string, string[]>();
            env["owin.ResponseBody"] = new MemoryStream();
            env["owin.CallCancelled"] = CancellationToken.None;

            return env;
        }
    }
}
