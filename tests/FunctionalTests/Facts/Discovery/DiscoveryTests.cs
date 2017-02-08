// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using FunctionalTests.Common;
using Microsoft.Owin;
using Owin;
using Xunit;
using Xunit.Extensions;

[assembly: OwinStartup(typeof(FunctionalTests.Startup))]

namespace FunctionalTests.Facts.Discovery
{
    public class DiscoveryTests
    {
        [Theory, Trait("FunctionalTests", "General")]
        [InlineData(HostType.IIS)]
        [InlineData(HostType.HttpListener)]
        public void DefaultDiscoveryTest(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                string applicationUrl = deployer.Deploy(hostType);
                string responseText = HttpClientUtility.GetResponseTextFromUrl(applicationUrl);
                Assert.Equal(Startup.RESULT, responseText);
            }
        }
    }
}

namespace FunctionalTests
{
    public class Startup
    {
        public const string RESULT = "Default Startup Discovered";

        public void Configuration(IAppBuilder app)
        {
            app.Run(context =>
                {
                    return context.Response.WriteAsync(RESULT);
                });
        }
    }
}