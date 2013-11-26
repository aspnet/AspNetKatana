// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.StaticFiles;
using Owin;
using Xunit;
using Xunit.Extensions;

#if NET40
namespace Microsoft.Owin.Host40.IntegrationTests
#else
namespace Microsoft.Owin.Host45.IntegrationTests
#endif
{
    // Note these tests require runAllManagedModulesForAllRequests for System.Web.
    public class StaticFilesTests : TestBase
    {
        public void DefaultStaticFiles(IAppBuilder app)
        {
            app.Use((context, next) => { context.Response.Headers["PassedThroughOWIN"] = "True"; return next(); });
            app.UseStaticFiles();
            app.Run(context => { context.Response.StatusCode = 402; return context.Response.WriteAsync("Fell Through"); });
            app.UseStageMarker(PipelineStage.MapHandler);
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task ExistingFiles_Served(string serverName)
        {
            int port = RunWebServer(
                serverName,
                DefaultStaticFiles);

            var client = new HttpClient();
            return client.GetAsync("http://localhost:" + port + "/Content/textfile.txt")
                .Then(response =>
                {
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    Assert.Equal("True", string.Join(",", response.Headers.GetValues("PassedThroughOWIN")));
                });
        }

        // This test must run at MapHandler or earlier on System.Web.  Otherwise the native module returns 404.
        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task NotFound_PassThrough(string serverName)
        {
            int port = RunWebServer(
                serverName,
                DefaultStaticFiles);

            var client = new HttpClient();
            return client.GetAsync("http://localhost:" + port + "/Content/doesntexist.txt")
                .Then(response =>
                {
                    Assert.Equal(HttpStatusCode.PaymentRequired, response.StatusCode);
                    Assert.Equal("True", string.Join(",", response.Headers.GetValues("PassedThroughOWIN")));
                });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task SingleRange_RangeServed(string serverName)
        {
            int port = RunWebServer(
                serverName,
                DefaultStaticFiles);

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Range", "bytes=10-20");
            return client.GetAsync("http://localhost:" + port + "/Content/textfile.txt")
                .Then(response =>
                {
                    Assert.Equal(HttpStatusCode.PartialContent, response.StatusCode);
                    Assert.Equal("True", string.Join(",", response.Headers.GetValues("PassedThroughOWIN")));
                    Assert.Equal(11, response.Content.Headers.ContentLength);
                });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task TwoRanges_ServeFull(string serverName)
        {
            int port = RunWebServer(
                serverName,
                DefaultStaticFiles);

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Range", "bytes=10-20,22-30");
            return client.GetAsync("http://localhost:" + port + "/Content/textfile.txt")
                .Then(response =>
                {
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    Assert.Equal("True", string.Join(",", response.Headers.GetValues("PassedThroughOWIN")));
                });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task ManyRanges_ServeFull(string serverName)
        {
            int port = RunWebServer(
                serverName,
                DefaultStaticFiles);

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Range", "bytes=0-0,12-12,14-14,16-16,2-2,4-4,6-6,8-8,10-10");
            return client.GetAsync("http://localhost:" + port + "/Content/textfile.txt")
                .Then(response =>
                {
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    Assert.Equal("True", string.Join(",", response.Headers.GetValues("PassedThroughOWIN")));
                    Assert.Equal("text/plain", response.Content.Headers.ContentType.ToString());
                });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public void TooManyRanges_ServeFull(string serverName)
        {
            int port = RunWebServer(
                serverName,
                DefaultStaticFiles);

            // There's a bug in the 4.0 HttpClientHandler for unbounded ('10-') range header values.  Use HWR instead.
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost:" + port + "/Content/textfile.txt");
            request.AddRange(10);
            request.AddRange(10);
            request.AddRange(10);
            request.AddRange(10);
            request.AddRange(10);
            request.AddRange(10);
            request.AddRange(10);
            request.AddRange(10);
            request.AddRange(10);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("True", string.Join(",", response.Headers.GetValues("PassedThroughOWIN")));
            Assert.Equal("text/plain", response.ContentType);
        }

        public void DirectoryBrowser(IAppBuilder app)
        {
            app.Use((context, next) => { context.Response.Headers["PassedThroughOWIN"] = "True"; return next(); });
            app.UseDirectoryBrowser();
            app.Run(context => { context.Response.StatusCode = 402; return context.Response.WriteAsync("Fell Through"); });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task ExistingDirectory_Served(string serverName)
        {
            int port = RunWebServer(
                serverName,
                DirectoryBrowser);

            var client = new HttpClient();
            return client.GetAsync("http://localhost:" + port + "/")
                .Then(response =>
                {
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    Assert.Equal("True", string.Join(",", response.Headers.GetValues("PassedThroughOWIN")));
                });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task ExistingSubDirectory_Served(string serverName)
        {
            int port = RunWebServer(
                serverName,
                DirectoryBrowser);

            var client = new HttpClient();
            return client.GetAsync("http://localhost:" + port + "/Content/")
                .Then(response =>
                {
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    Assert.Equal("True", string.Join(",", response.Headers.GetValues("PassedThroughOWIN")));
                });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task NonExistingSubDirectory_Served(string serverName)
        {
            int port = RunWebServer(
                serverName,
                DirectoryBrowser);

            var client = new HttpClient();
            return client.GetAsync("http://localhost:" + port + "/doesntexist/")
                .Then(response =>
                {
                    Assert.Equal(HttpStatusCode.PaymentRequired, response.StatusCode);
                    Assert.Equal("True", string.Join(",", response.Headers.GetValues("PassedThroughOWIN")));
                });
        }

        public void DefaultFile(IAppBuilder app)
        {
            app.Use((context, next) => { context.Response.Headers["PassedThroughOWIN"] = "True"; return next(); });
            DefaultFilesOptions options = new DefaultFilesOptions();
            options.DefaultFileNames.Add("TextFile.txt");
            app.UseDefaultFiles(options);
            app.Run(context => { context.Response.StatusCode = 402; return context.Response.WriteAsync(context.Request.Path.Value); });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task NoDefaultFile_PassThrough(string serverName)
        {
            int port = RunWebServer(
                serverName,
                DefaultFile);

            var client = new HttpClient();
            return client.GetAsync("http://localhost:" + port + "/")
                .Then(response =>
                {
                    Assert.Equal(HttpStatusCode.PaymentRequired, response.StatusCode);
                    Assert.Equal("True", string.Join(",", response.Headers.GetValues("PassedThroughOWIN")));
                    Assert.Equal("/", response.Content.ReadAsStringAsync().Result);
                });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task DefaultFile_PathChanged(string serverName)
        {
            int port = RunWebServer(
                serverName,
                DefaultFile);

            var client = new HttpClient();
            return client.GetAsync("http://localhost:" + port + "/Content/")
                .Then(response =>
                {
                    Assert.Equal(HttpStatusCode.PaymentRequired, response.StatusCode);
                    Assert.Equal("True", string.Join(",", response.Headers.GetValues("PassedThroughOWIN")));
                    Assert.Equal("/Content/TextFile.txt", response.Content.ReadAsStringAsync().Result);
                });
        }
    }
}
