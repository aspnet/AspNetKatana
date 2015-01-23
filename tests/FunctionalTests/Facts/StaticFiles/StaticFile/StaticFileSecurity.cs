// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using FunctionalTests.Common;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;
using Xunit;
using Xunit.Extensions;

namespace FunctionalTests.Facts.StaticFiles
{
    public class StaticFileSecurity
    {
        [Theory, Trait("FunctionalTests", "General")]
        [InlineData(HostType.HttpListener)]
        public void Static_BlockedFiles(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                var applicationUrl = deployer.Deploy(hostType, BlockedFiles_Configuration);

                var response = HttpClientUtility.GetResponseTextFromUrl(applicationUrl + "TextFile.txt");
                Assert.Equal<string>(@"BlockedFiles\TextFile", response);

                //Clock$ should not be served. This is the only file among the list of blocked files, I can create on the disk.
                response = HttpClientUtility.GetResponseTextFromUrl(applicationUrl + "clock$.txt");
                Assert.Equal<string>("FallThrough", response);
            }
        }

        public void BlockedFiles_Configuration(IAppBuilder app)
        {
            app.UseStaticFiles(new StaticFileOptions() { FileSystem = new PhysicalFileSystem(@"RequirementFiles\BlockedFiles") });
            app.Run(context => context.Response.WriteAsync("FallThrough"));
        }
    }
}