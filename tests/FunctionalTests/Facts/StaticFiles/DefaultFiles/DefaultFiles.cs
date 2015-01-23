// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
    public class DefaultFiles
    {
        [Theory, Trait("FunctionalTests", "General")]
        [InlineData(HostType.HttpListener)]
        public void Static_DefaultFilesDefaultSetup(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                string applicationUrl = deployer.Deploy(hostType, FolderWithDefaultFileConfiguration);

                HttpResponseMessage response = null;

                /*GET requests*/
                //Directory with no default file - request path ending with '/'
                HttpClientUtility.GetResponseTextFromUrl(applicationUrl + "RequirementFiles/", out response);
                Assert.Equal<HttpStatusCode>(HttpStatusCode.NotFound, response.StatusCode);

                //Directory with no default file - request path not ending with '/'
                HttpClientUtility.GetResponseTextFromUrl(applicationUrl + "RequirementFiles", out response);
                Assert.Equal<HttpStatusCode>(HttpStatusCode.NotFound, response.StatusCode);

                //Directory with a default file - request path ending with a '/'
                var responseText = HttpClientUtility.GetResponseTextFromUrl(applicationUrl + "RequirementFiles/Dir1/", out response);
                Assert.Equal<HttpStatusCode>(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal<bool>(true, responseText.Contains(@"Dir1\Default.html"));

                //Directory with a default file - request path not ending with a '/'
                responseText = HttpClientUtility.GetResponseTextFromUrl(applicationUrl + "RequirementFiles/Dir1", out response);
                Assert.Equal<HttpStatusCode>(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal<bool>(true, responseText.Contains(@"Dir1\Default.html"));

                //Directory with a default file - request path ending with a '/' & case sensitivity check
                responseText = HttpClientUtility.GetResponseTextFromUrl(applicationUrl + "reQuirementFiles/dir1/", out response);
                Assert.Equal<HttpStatusCode>(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal<bool>(true, responseText.Contains(@"Dir1\Default.html"));

                /*HEAD requests*/
                //Directory with no default file - request path ending with '/'
                responseText = HttpClientUtility.HeadResponseTextFromUrl(applicationUrl + "RequirementFiles/", out response);
                Assert.Equal<HttpStatusCode>(HttpStatusCode.NotFound, response.StatusCode);

                //Directory with no default file - request path not ending with '/'
                responseText = HttpClientUtility.HeadResponseTextFromUrl(applicationUrl + "RequirementFiles", out response);
                Assert.Equal<HttpStatusCode>(HttpStatusCode.NotFound, response.StatusCode);

                //Directory with a default file - request path ending with a '/'
                responseText = HttpClientUtility.HeadResponseTextFromUrl(applicationUrl + "RequirementFiles/Dir1/", out response);
                Assert.Equal<HttpStatusCode>(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal<string>(string.Empty, responseText);

                //Directory with a default file - request path not ending with a '/'
                responseText = HttpClientUtility.HeadResponseTextFromUrl(applicationUrl + "RequirementFiles/Dir1", out response);
                Assert.Equal<HttpStatusCode>(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal<string>(string.Empty, responseText);

                //Directory with a default file - request path ending with a '/' & case sensitivity check
                responseText = HttpClientUtility.HeadResponseTextFromUrl(applicationUrl + "reQuirementFiles/dir1/", out response);
                Assert.Equal<HttpStatusCode>(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal<string>(string.Empty, responseText);

                /*POST requests - no file to be served*/
                //Directory with no default file - request path ending with '/'
                HttpClientUtility.PostResponseTextFromUrl(applicationUrl + "RequirementFiles/", out response);
                Assert.Equal<HttpStatusCode>(HttpStatusCode.NotFound, response.StatusCode);

                //Directory with a default file - request path not ending with a '/'
                HttpClientUtility.PostResponseTextFromUrl(applicationUrl + "RequirementFiles/Dir1/", out response);
                Assert.Equal<HttpStatusCode>(HttpStatusCode.NotFound, response.StatusCode);
            }
        }

        public void FolderWithDefaultFileConfiguration(IAppBuilder app)
        {
            app.UseDefaultFiles();
            app.UseStaticFiles();
        }

        [Theory, Trait("FunctionalTests", "General")]
        [InlineData(HostType.HttpListener)]
        public void Static_CustomRequestPathToPhysicalPathMapping(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                string applicationUrl = deployer.Deploy(hostType, CustomRequestPathToPhysicalPathMappingConfiguration);

                HttpResponseMessage response = null;

                //Directory with a default file - request path ending with a '/'. A local directory referred by relative path
                var responseText = HttpClientUtility.GetResponseTextFromUrl(applicationUrl + "customrequestPath/", out response);
                Assert.Equal<HttpStatusCode>(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal<bool>(true, responseText.Contains(@"Dir1\Default.html"));

                //Directory with a default file - request path ending with a '/' + Head request. A local directory referred by relative path
                responseText = HttpClientUtility.HeadResponseTextFromUrl(applicationUrl + "customrequestpath/", out response);
                Assert.Equal<HttpStatusCode>(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal<string>(string.Empty, responseText);

                //Directory with a default file - request path ending with a '/'. A local directory referred by absolute path
                responseText = HttpClientUtility.GetResponseTextFromUrl(applicationUrl + "customrequestfullPath/", out response);
                Assert.Equal<HttpStatusCode>(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal<bool>(true, responseText.Contains(@"Dir2\TextFile2.txt"));

                //Directory with a default file - request path ending with a '/' + Head request. A local directory referred by absolute path
                responseText = HttpClientUtility.HeadResponseTextFromUrl(applicationUrl + "customrequestFullPath/", out response);
                Assert.Equal<HttpStatusCode>(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal<string>(string.Empty, responseText);

                //Directory with a default file - request path ending with a '/'. Mapped to a UNC path.
                responseText = HttpClientUtility.GetResponseTextFromUrl(applicationUrl + "customrequestuncPath/", out response);
                Assert.Equal<HttpStatusCode>(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal<bool>(true, responseText.Contains(@"Dir3\TextFile3.txt"));

                //Directory with a default file - request path ending with a '/' + Head request. Mapped to a UNC path.
                responseText = HttpClientUtility.HeadResponseTextFromUrl(applicationUrl + "customrequestUNCPath/", out response);
                Assert.Equal<HttpStatusCode>(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal<string>(string.Empty, responseText);
            }
        }

        public void CustomRequestPathToPhysicalPathMappingConfiguration(IAppBuilder app)
        {
            //Use relative path from root of the application
            app.UseFileServer(new FileServerOptions() { RequestPath = new PathString("/customrequestPath"), FileSystem = new PhysicalFileSystem(@"RequirementFiles\Dir1") });

            //Use disk absolute path for specifying the serving directory.
            var absolutePathOptions = new FileServerOptions() { RequestPath = new PathString("/customrequestFullPath"), FileSystem = new PhysicalFileSystem(Path.Combine(Environment.CurrentDirectory, @"RequirementFiles\Dir2")) };
            absolutePathOptions.DefaultFilesOptions.DefaultFileNames = new string[] { "TextFile2.txt" };
            app.UseFileServer(absolutePathOptions);

            //Use UNC path - serving files from a shared UNC path.
            var uncPathOptions = new FileServerOptions()
            {
                RequestPath = new PathString("/customrequestUNCPath"),
                FileSystem = new PhysicalFileSystem(Path.Combine("\\\\", Environment.MachineName, Path.Combine(Environment.CurrentDirectory, @"RequirementFiles\Dir3").Replace(':', '$')))
            };
            uncPathOptions.DefaultFilesOptions.DefaultFileNames = new string[] { "TextFile3.txt" };
            app.UseFileServer(uncPathOptions);
        }
    }
}