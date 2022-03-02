﻿// Copyright (c) .NET Foundation. All rights reserved.
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
    public class EmbeddedDirectoryBrowser
    {
        [Theory, Trait("FunctionalTests", "General")]
        [InlineData(HostType.HttpListener)]
        public void Static_EmbeddedDirectoryBrowserFileSystem(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                var applicationUrl = deployer.Deploy(hostType, EmbeddedDirectoryBrowserFileSystemConfiguration);

                HttpResponseMessage response = null;

                var responseText = HttpClientUtility.GetResponseTextFromUrl(applicationUrl, out response);
                Assert.True(!string.IsNullOrWhiteSpace(responseText), "Received empty response");
                Assert.True((response.Content).Headers.ContentType.ToString() == "text/html; charset=utf-8");
                Assert.Contains("RequirementFiles.EmbeddedResources.SampleAVI.avi", responseText);
            }
        }

        internal void EmbeddedDirectoryBrowserFileSystemConfiguration(IAppBuilder app)
        {
            app.UseDirectoryBrowser(new DirectoryBrowserOptions() { FileSystem = new EmbeddedResourceFileSystem(Assembly.GetExecutingAssembly().GetName().Name) });
        }
    }
}
