// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using FunctionalTests.Common;
using Microsoft.Owin.Builder;
using Owin;
using Xunit;
using Xunit.Extensions;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace FunctionalTests.Facts.Discovery
{
    public class AllowedNonDefaultConfigurationSignatures2
    {
        [Theory, Trait("FunctionalTests", "General")]
        [InlineData(HostType.IIS)]
        [InlineData(HostType.HttpListener)]
        public void AllowedConfigurationSignature2(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                string applicationUrl = deployer.Deploy<AllowedNonDefaultConfigurationSignatures2>(hostType);
                Assert.Equal("SUCCESS", HttpClientUtility.GetResponseTextFromUrl(applicationUrl));
            }
        }

        public object Configuration()
        {
            var builder = new AppBuilder();
            builder.Run(context =>
                    {
                        return context.Response.WriteAsync("SUCCESS");
                    });
            return builder.Build<AppFunc>();
        }
    }
}