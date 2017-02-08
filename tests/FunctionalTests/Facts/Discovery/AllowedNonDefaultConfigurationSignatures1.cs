// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using FunctionalTests.Common;
using Microsoft.Owin.Builder;
using Owin;
using Xunit;
using Xunit.Extensions;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace FunctionalTests.Facts.Discovery
{
    public class AllowedNonDefaultConfigurationSignatures1
    {
        [Theory, Trait("FunctionalTests", "General")]
        [InlineData(HostType.IIS)]
        [InlineData(HostType.HttpListener)]
        public void AllowedConfigurationSignature1(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                string applicationUrl = deployer.Deploy<AllowedNonDefaultConfigurationSignatures1>(hostType);
                Assert.Equal("SUCCESS", HttpClientUtility.GetResponseTextFromUrl(applicationUrl));
            }
        }

        public object Configuration(IDictionary<string, object> properties)
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
