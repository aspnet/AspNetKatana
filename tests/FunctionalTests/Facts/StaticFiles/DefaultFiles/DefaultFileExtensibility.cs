// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using FunctionalTests.Common;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;
using Xunit;
using Xunit.Extensions;

namespace FunctionalTests.Facts.StaticFiles
{
    public class DefaultFileExtensibility
    {
        [Theory, Trait("FunctionalTests", "General")]
        [InlineData(HostType.HttpListener)]
        public void Static_CustomDefaultFile(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                string applicationUrl = deployer.Deploy(hostType, CustomDefaultFileConfiguration);

                HttpResponseMessage response = null;

                //Directory with a default file - case request path ending with a '/'
                var responseText = HttpClientUtility.GetResponseTextFromUrl(applicationUrl, out response);
                Assert.Equal<HttpStatusCode>(HttpStatusCode.OK, response.StatusCode);
                Assert.Contains(@"Dir1\Textfile1.txt", responseText);

                //Directory with a default file - case request path ending with a '/' + Head request
                responseText = HttpClientUtility.HeadResponseTextFromUrl(applicationUrl, out response);
                Assert.Equal<HttpStatusCode>(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(string.Empty, responseText);
            }
        }

        internal void CustomDefaultFileConfiguration(IAppBuilder app)
        {
            var options = new DefaultFilesOptions() { DefaultFileNames = new string[] { "TextFile1.txt" }, FileSystem = new PhysicalFileSystem(@"RequirementFiles\Dir1") };
            app.UseDefaultFiles(options);

            app.UseStaticFiles(new StaticFileOptions()
            {
                FileSystem = new PhysicalFileSystem(@"RequirementFiles\Dir1")
            });
        }
    }
}