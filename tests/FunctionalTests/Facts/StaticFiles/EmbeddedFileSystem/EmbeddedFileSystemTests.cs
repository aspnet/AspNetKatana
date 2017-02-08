// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Reflection;
using FunctionalTests.Common;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;
using Xunit;
using Xunit.Extensions;

namespace FunctionalTests.Facts.StaticFiles.EmbeddedFileSystem
{
    public class EmbeddedFileSystemTests
    {
        [Theory, Trait("FunctionalTests", "General")]
        [InlineData(HostType.HttpListener)]
        public void Static_EmbeddedFileSystem(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                var applicationUrl = deployer.Deploy(hostType, EmbeddedFileSystemConfiguration);
                var httpClient = new HttpClient() { BaseAddress = new Uri(applicationUrl) };

                DownloadAndCompareFiles(httpClient, "RequirementFiles.EmbeddedResources.SampleAVI.avi", "video/x-msvideo");
                DownloadAndCompareFiles(httpClient, "RequirementFiles.EmbeddedResources.SampleC.c", "text/plain");
                DownloadAndCompareFiles(httpClient, "RequirementFiles.EmbeddedResources.SampleCHM.CHM", "application/octet-stream");
                DownloadAndCompareFiles(httpClient, "RequirementFiles.EmbeddedResources.SampleCPP.cpp", "text/plain");
                DownloadAndCompareFiles(httpClient, "RequirementFiles.EmbeddedResources.SampleCSS.css", "text/css");
                DownloadAndCompareFiles(httpClient, "RequirementFiles.EmbeddedResources.SampleCSV.csv", "application/octet-stream");
                DownloadAndCompareFiles(httpClient, "RequirementFiles.EmbeddedResources.SampleCUR.cur", "application/octet-stream");
                DownloadAndCompareFiles(httpClient, "RequirementFiles.EmbeddedResources.SampleDISCO.disco", "text/xml");
                DownloadAndCompareFiles(httpClient, "RequirementFiles.EmbeddedResources.SampleDOC.DOC", "application/msword");
                DownloadAndCompareFiles(httpClient, "RequirementFiles.EmbeddedResources.SampleDOCX.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
                DownloadAndCompareFiles(httpClient, "RequirementFiles.EmbeddedResources.SampleHTM.htm", "text/html");
                DownloadAndCompareFiles(httpClient, "RequirementFiles.EmbeddedResources.SampleHTML.html", "text/html");
                DownloadAndCompareFiles(httpClient, "RequirementFiles.EmbeddedResources.SampleICO.ico", "image/x-icon");
                DownloadAndCompareFiles(httpClient, "RequirementFiles.EmbeddedResources.SampleJPEG.jpg", "image/jpeg");
                DownloadAndCompareFiles(httpClient, "RequirementFiles.EmbeddedResources.SampleJPG.jpg", "image/jpeg");
                DownloadAndCompareFiles(httpClient, "RequirementFiles.EmbeddedResources.SamplePNG.png", "image/png");

                //Unknown MIME types should not be served by default
                var response = httpClient.GetAsync("RequirementFiles.EmbeddedResources.Unknown.Unknown").Result;
                Assert.Equal<HttpStatusCode>(HttpStatusCode.NotFound, response.StatusCode);
            }
        }

        public void EmbeddedFileSystemConfiguration(IAppBuilder app)
        {
            var options = new StaticFileOptions() { FileSystem = new EmbeddedResourceFileSystem(Assembly.GetExecutingAssembly().GetName().Name) };
            app.UseStaticFiles(options);
        }

        private void DownloadAndCompareFiles(HttpClient httpClient, string fileName, string expectedMimeType)
        {
            var response = httpClient.GetAsync(fileName).Result;
            Assert.Equal<string>(expectedMimeType, response.Content.Headers.ContentType.MediaType);

            var testAssembly = Assembly.GetExecutingAssembly();

            using (var baselineStream = testAssembly.GetManifestResourceStream(testAssembly.GetName().Name + "." + fileName))
            {
                var byteCount = 0;

                while (true)
                {
                    var baselineByte = baselineStream.ReadByte();
                    var contentByte = response.Content.ReadAsStreamAsync().Result.ReadByte();
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
    }
}