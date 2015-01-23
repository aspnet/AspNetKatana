// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using FunctionalTests.Common;
using Microsoft.Owin.StaticFiles;
using Microsoft.Owin.StaticFiles.ContentTypes;
using Owin;
using Xunit;
using Xunit.Extensions;

namespace FunctionalTests.Facts.StaticFiles
{
    public class StaticFile
    {
        [Theory, Trait("FunctionalTests", "General")]
        [InlineData(HostType.HttpListener)]
        public void Static_ContentTypes(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                var applicationUrl = deployer.Deploy(hostType, ContentTypesConfiguration);
                var httpClient = new HttpClient() { BaseAddress = new Uri(applicationUrl) };

                DownloadAndCompareFiles(httpClient, @"RequirementFiles\ContentTypes\SampleAVI.avi", "video/x-msvideo");
                DownloadAndCompareFiles(httpClient, @"RequirementFiles\ContentTypes\SampleC.c", "text/plain");
                DownloadAndCompareFiles(httpClient, @"RequirementFiles\ContentTypes\SampleCHM.chm", "application/octet-stream");
                DownloadAndCompareFiles(httpClient, @"RequirementFiles\ContentTypes\SampleCpp.cpp", "text/plain");
                DownloadAndCompareFiles(httpClient, @"RequirementFiles\ContentTypes\SampleCss.CSS", "text/css");
                DownloadAndCompareFiles(httpClient, @"RequirementFiles\ContentTypes\SampleCSV.csV", "application/octet-stream");
                DownloadAndCompareFiles(httpClient, @"RequirementFiles\ContentTypes\SampleCUR.cur", "application/octet-stream");
                DownloadAndCompareFiles(httpClient, @"RequirementFiles\ContentTypes\SampleDisco.disco", "text/xml");
                DownloadAndCompareFiles(httpClient, @"RequirementFiles\ContentTypes\Sampledoc.doc", "application/msword");
                DownloadAndCompareFiles(httpClient, @"RequirementFiles\ContentTypes\Sampledocx.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
                DownloadAndCompareFiles(httpClient, @"RequirementFiles\ContentTypes\SampleHTM.htm", "text/html");
                DownloadAndCompareFiles(httpClient, @"RequirementFiles\ContentTypes\Samplehtml.html", "text/html");
                DownloadAndCompareFiles(httpClient, @"RequirementFiles\ContentTypes\Sampleico.ico", "image/x-icon");
                DownloadAndCompareFiles(httpClient, @"RequirementFiles\ContentTypes\SampleJPEG.jpg", "image/jpeg");
                DownloadAndCompareFiles(httpClient, @"RequirementFiles\ContentTypes\SampleJPG.jpg", "image/jpeg");
                DownloadAndCompareFiles(httpClient, @"RequirementFiles\ContentTypes\SamplePNG.png", "image/png");
                DownloadAndCompareFiles(httpClient, @"RequirementFiles\Dir1\EmptyFile.txt", "text/plain");

                //Unknown MIME types should not be served by default
                var response = httpClient.GetAsync(@"RequirementFiles\ContentTypes\Unknown.Unknown").Result;
                Assert.Equal<HttpStatusCode>(HttpStatusCode.NotFound, response.StatusCode);
            }
        }

        private void DownloadAndCompareFiles(HttpClient httpClient, string fileName, string expectedMimeType)
        {
            var httpContent = httpClient.GetAsync(fileName).Result.Content;
            Assert.Equal<string>(expectedMimeType, httpContent.Headers.ContentType.MediaType);

            using (var baselineStream = new FileStream(fileName, FileMode.Open))
            {
                var byteCount = 0;

                while (true)
                {
                    var baselineByte = baselineStream.ReadByte();
                    var contentByte = httpContent.ReadAsStreamAsync().Result.ReadByte();
                    byteCount++;

                    Assert.Equal<int>(baselineByte, contentByte);

                    if (baselineByte == -1)
                    {
                        Trace.WriteLine(string.Format("Finished downloading and comparing the file '{0}' of length '{1}'", fileName, byteCount));
                        break;
                    }
                }
            }
        }

        public void ContentTypesConfiguration(IAppBuilder app)
        {
            app.UseStaticFiles();
        }

        [Theory, Trait("FunctionalTests", "General")]
        [InlineData(HostType.HttpListener)]
        public void Static_ServeUnknownFileTypes(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                var applicationUrl = deployer.Deploy(hostType, ServeUnknownFileTypesConfiguration);
                var httpClient = new HttpClient() { BaseAddress = new Uri(applicationUrl) };

                var response = httpClient.GetAsync("RequirementFiles/ContentTypes/Unknown.Unknown").Result;
                Assert.Equal<HttpStatusCode>(HttpStatusCode.NotFound, response.StatusCode);

                httpClient.DefaultRequestHeaders.Add("ServeUnknown", "true");
                response = httpClient.GetAsync("RequirementFiles/ContentTypes/Unknown.Unknown").Result;
                Assert.Equal<HttpStatusCode>(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal<string>(@"ContentTypes\Unknown.Unknown", response.Content.ReadAsStringAsync().Result);
                Assert.Equal<string>("unknown/unknown", response.Content.Headers.ContentType.MediaType);
            }
        }

        public void ServeUnknownFileTypesConfiguration(IAppBuilder app)
        {
            var staticFileOptions = new StaticFileOptions();

            app.Use((context, next) =>
                {
                    staticFileOptions.ServeUnknownFileTypes = context.Request.Headers.ContainsKey("ServeUnknown") ? true : false;
                    staticFileOptions.DefaultContentType = staticFileOptions.ServeUnknownFileTypes ? "unknown/unknown" : null;
                    return next.Invoke();
                });

            app.UseStaticFiles(staticFileOptions);
        }

        [Theory, Trait("FunctionalTests", "General")]
        [InlineData(HostType.HttpListener)]
        public void Static_CustomMimeType(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                var applicationUrl = deployer.Deploy(hostType, CustomMimeTypeConfiguration);
                var httpClient = new HttpClient() { BaseAddress = new Uri(applicationUrl) };

                var response = httpClient.GetAsync(@"RequirementFiles\ContentTypes\Unknown.unknown").Result;
                Assert.Equal<string>("CustomUnknown", string.Join("", response.Content.Headers.GetValues("Content-Type")));
            }
        }

        public void CustomMimeTypeConfiguration(IAppBuilder app)
        {
            var options = new StaticFileOptions();
            (options.ContentTypeProvider as FileExtensionContentTypeProvider).Mappings.Add(".Unknown", "CustomUnknown");
            app.UseStaticFiles(options);
        }

        [Theory, Trait("FunctionalTests", "General")]
        [InlineData(HostType.HttpListener)]
        public void Static_CustomMimeTypeProvider(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                var applicationUrl = deployer.Deploy(hostType, CustomMimeTypeProviderConfiguration);
                var httpClient = new HttpClient() { BaseAddress = new Uri(applicationUrl) };

                var response = httpClient.GetAsync(@"RequirementFiles\ContentTypes\Unknown.unknown").Result;
                Assert.Equal<string>("CustomMimeTypeProvider", string.Join("", response.Content.Headers.GetValues("Content-Type")));

                response = httpClient.GetAsync(@"RequirementFiles\ContentTypes\SamplePNG.png").Result;
                Assert.Equal<string>("Hello", string.Join("", response.Content.Headers.GetValues("Content-Type")));
            }
        }

        public void CustomMimeTypeProviderConfiguration(IAppBuilder app)
        {
            app.UseStaticFiles(new StaticFileOptions() { ContentTypeProvider = new CustomMimeTypeProvider(), ServeUnknownFileTypes = true, DefaultContentType = "Hello" });
        }

        public class CustomMimeTypeProvider : IContentTypeProvider
        {
            public bool TryGetContentType(string subpath, out string contentType)
            {
                contentType = null;
                if (subpath.EndsWith("Unknown.unknown"))
                {
                    contentType = "CustomMimeTypeProvider";
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        [Theory, Trait("FunctionalTests", "General")]
        [InlineData(HostType.HttpListener)]
        public void Static_OnPrepareResponseTest(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                var applicationUrl = deployer.Deploy(hostType, OnPrepareResponseTestConfiguration);
                var httpClient = new HttpClient() { BaseAddress = new Uri(applicationUrl) };

                var response = httpClient.GetAsync(@"RequirementFiles\Dir1\Default.html").Result;
                Assert.Equal<string>("true", string.Join("", response.Headers.GetValues("CallBackInvoked")));
                Assert.Equal<bool>(true, response.Content.ReadAsStringAsync().Result.Contains(@"Dir1\Default.html"));
            }
        }

        public void OnPrepareResponseTestConfiguration(IAppBuilder app)
        {
            Action<StaticFileResponseContext> onPrepareResponseCallBack = context =>
                {
                    Assert.Equal<string>("Default.html", context.File.Name);
                    context.OwinContext.Response.Headers.Add("CallBackInvoked", new string[] { "true" });
                };

            app.UseStaticFiles(new StaticFileOptions() { OnPrepareResponse = onPrepareResponseCallBack });
            app.UseStaticFiles();
        }
    }
}