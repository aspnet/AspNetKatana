// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using FunctionalTests.Common;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;
using Xunit;
using Xunit.Extensions;

namespace FunctionalTests.Facts.StaticFiles
{
    public class DirectoryBrowser
    {
        [Theory, Trait("FunctionalTests", "General")]
        [InlineData(HostType.HttpListener)]
        public void Static_DirectoryBrowserDefaults(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                string applicationUrl = deployer.Deploy(hostType, DirectoryBrowserDefaultsConfiguration);

                HttpResponseMessage response = null;

                //1. Check directory browsing enabled at application level
                var responseText = HttpClientUtility.GetResponseTextFromUrl(applicationUrl, out response);
                Assert.True(!string.IsNullOrWhiteSpace(responseText), "Received empty response");
                Assert.True((response.Content).Headers.ContentType.ToString() == "text/html; charset=utf-8");
                Assert.Contains("RequirementFiles/", responseText);

                //2. Check directory browsing @RequirementFiles with a ending '/'
                responseText = HttpClientUtility.GetResponseTextFromUrl(applicationUrl + "RequirementFiles/", out response);
                Assert.True(!string.IsNullOrWhiteSpace(responseText), "Received empty response");
                Assert.True((response.Content).Headers.ContentType.ToString() == "text/html; charset=utf-8");
                Assert.True(responseText.Contains("Dir1/") && responseText.Contains("Dir2/") && responseText.Contains("Dir3/"), "Directories Dir1, Dir2, Dir3 not found");

                //2. Check directory browsing @RequirementFiles with request path not ending '/'
                responseText = HttpClientUtility.GetResponseTextFromUrl(applicationUrl + "RequirementFiles", out response);
                Assert.True(!string.IsNullOrWhiteSpace(responseText), "Received empty response");
                Assert.True((response.Content).Headers.ContentType.ToString() == "text/html; charset=utf-8");
                Assert.True(responseText.Contains("Dir1/") && responseText.Contains("Dir2/") && responseText.Contains("Dir3/"), "Directories Dir1, Dir2, Dir3 not found");
            }
        }

        internal void DirectoryBrowserDefaultsConfiguration(IAppBuilder app)
        {
            app.UseDirectoryBrowser();
        }

        [Theory, Trait("FunctionalTests", "General")]
        [InlineData(HostType.HttpListener)]
        public void Static_DirectoryMiddlewareMappedToDifferentDirectory(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                string applicationUrl = deployer.Deploy(hostType, DirectoryMiddlewareMappedToDifferentDirectoryConfiguration);

                HttpResponseMessage response = null;

                //1. Check directory browsing enabled at application level
                var responseText = HttpClientUtility.GetResponseTextFromUrl(applicationUrl, out response);
                Assert.True(!string.IsNullOrWhiteSpace(responseText), "Received empty response");
                Assert.True((response.Content).Headers.ContentType.ToString() == "text/html; charset=utf-8");
                Assert.Contains("Default.html", responseText);
                Assert.Contains("EmptyFile.txt", responseText);
            }
        }

        internal void DirectoryMiddlewareMappedToDifferentDirectoryConfiguration(IAppBuilder app)
        {
            app.UseDirectoryBrowser(new DirectoryBrowserOptions() { FileSystem = new PhysicalFileSystem(@"RequirementFiles\Dir1") });
        }

        [Theory, Trait("FunctionalTests", "General")]
        [InlineData(HostType.HttpListener)]
        public void Static_DirectoryCustomRequestPathToPhysicalPathMapping(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                string applicationUrl = deployer.Deploy(hostType, DirectoryCustomRequestPathToPhysicalPathMappingConfiguration);

                HttpResponseMessage response = null;

                //Directory with a default file - case request path ending with a '/'. A local directory referred by relative path
                var responseText = HttpClientUtility.GetResponseTextFromUrl(applicationUrl + "customrequestPath/", out response);
                Assert.Equal<HttpStatusCode>(HttpStatusCode.OK, response.StatusCode);
                Assert.True(!string.IsNullOrWhiteSpace(responseText), "Received empty response");
                Assert.True((response.Content).Headers.ContentType.ToString() == "text/html; charset=utf-8");
                Assert.Contains("Unknown.Unknown", responseText);
                Assert.Contains("Default.html", responseText);

                //Directory with a default file - case request path ending with a '/' + Head request. A local directory referred by relative path
                responseText = HttpClientUtility.HeadResponseTextFromUrl(applicationUrl + "customrequestPath/", out response);
                Assert.Equal<HttpStatusCode>(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(string.Empty, responseText);

                //Directory with a default file - case request path ending with a '/'. A local directory referred by absolute path
                responseText = HttpClientUtility.GetResponseTextFromUrl(applicationUrl + "customrequestFullPath/", out response);
                Assert.Equal<HttpStatusCode>(HttpStatusCode.OK, response.StatusCode);
                Assert.Contains("TextFile2.txt", responseText);
                Assert.Contains("Unknown.Unknown", responseText);

                //Directory with a default file - case request path ending with a '/' + Head request. A local directory referred by absolute path
                responseText = HttpClientUtility.HeadResponseTextFromUrl(applicationUrl + "customrequestFullPath/", out response);
                Assert.Equal<HttpStatusCode>(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(string.Empty, responseText);

                //Directory with a default file - case request path ending with a '/'. Mapped to a UNC path.
                responseText = HttpClientUtility.GetResponseTextFromUrl(applicationUrl + "customrequestUNCPath/", out response);
                Assert.Equal<HttpStatusCode>(HttpStatusCode.OK, response.StatusCode);
                Assert.Contains("Dir31", responseText);
                Assert.Contains("Dir32", responseText);
                Assert.Contains("TextFile3.txt", responseText);
                Assert.Contains("TextFile4.txt", responseText);

                //Directory with a default file - case request path ending with a '/' + Head request. Mapped to a UNC path.
                responseText = HttpClientUtility.HeadResponseTextFromUrl(applicationUrl + "customrequestUNCPath/", out response);
                Assert.Equal<HttpStatusCode>(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(string.Empty, responseText);
            }
        }

        internal void DirectoryCustomRequestPathToPhysicalPathMappingConfiguration(IAppBuilder app)
        {
            app.UseDirectoryBrowser(new DirectoryBrowserOptions()
            {
                RequestPath = new PathString("/customrequestPath"),
                FileSystem = new PhysicalFileSystem(@"RequirementFiles\Dir1")
            });

            app.UseDirectoryBrowser(new DirectoryBrowserOptions()
            {
                RequestPath = new PathString("/customrequestFullPath"),
                FileSystem = new PhysicalFileSystem(Path.Combine(Environment.CurrentDirectory, @"RequirementFiles\Dir2"))
            });

            var localAbsolutePath = Path.Combine(Environment.CurrentDirectory, @"RequirementFiles\Dir3");
            var uncPath = Path.Combine("\\\\", Environment.MachineName, localAbsolutePath.Replace(':', '$'));
            app.UseDirectoryBrowser(new DirectoryBrowserOptions()
            {
                RequestPath = new PathString("/customrequestUNCPath"),
                FileSystem = new PhysicalFileSystem(uncPath)
            });
        }
    }
}