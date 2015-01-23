// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using FunctionalTests.Common;
using Owin;
using Xunit;
using Xunit.Extensions;

namespace FunctionalTests.Facts.Diagnostics
{
    public class WelcomePageFacts
    {
        [Theory, Trait("FunctionalTests", "General")]
        [InlineData(HostType.IIS)]
        [InlineData(HostType.HttpListener)]
        public void WelcomePage(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                string applicationUrl = deployer.Deploy(hostType, WelcomePageConfiguration);

                HttpResponseMessage response;
                Assert.True(HttpClientUtility.GetResponseTextFromUrl(applicationUrl, out response).ToLower().Contains("your owin application has been successfully started"));
                Assert.Equal("text/html", response.Content.Headers.ContentType.MediaType.ToLower());
            }
        }

        public static void WelcomePageConfiguration(IAppBuilder app)
        {
            app.UseWelcomePage();
        }
    }
}
