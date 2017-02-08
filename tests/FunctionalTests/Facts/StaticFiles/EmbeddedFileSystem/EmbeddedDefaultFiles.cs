// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
    public class EmbeddedDefaultFiles
    {
        [Theory, Trait("FunctionalTests", "General")]
        [InlineData(HostType.HttpListener)]
        public void Static_EmbeddedFileSystemDefaultFiles(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                var applicationUrl = deployer.Deploy(hostType, EmbeddedFileSystemDefaultFilesConfiguration);

                HttpResponseMessage response = null;

                var responseText = HttpClientUtility.GetResponseTextFromUrl(applicationUrl, out response);
                Assert.True(!string.IsNullOrWhiteSpace(responseText), "Received empty response");
                Assert.True((response.Content).Headers.ContentType.ToString() == "text/html");
                Assert.True(responseText.Contains("SampleHTM"));
            }
        }

        public void EmbeddedFileSystemDefaultFilesConfiguration(IAppBuilder app)
        {
            FileServerOptions options = new FileServerOptions();
            options.FileSystem = new EmbeddedResourceFileSystem(Assembly.GetExecutingAssembly().GetName().Name);
            options.DefaultFilesOptions.DefaultFileNames.Clear();
            options.DefaultFilesOptions.DefaultFileNames.Add("RequirementFiles.EmbeddedResources.SampleHTM.htm");

            app.UseFileServer(options);
        }
    }
}
