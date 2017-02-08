// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using FunctionalTests.Common;
using Xunit;
using Xunit.Extensions;

namespace FunctionalTests.Facts.Discovery
{
    public class ConfigurationMethodNotFoundTest
    {
        [Theory, Trait("FunctionalTests", "General")]
        [InlineData(HostType.IIS)]
        [InlineData(HostType.HttpListener)]
        public void ConfigurationMethodNotFound(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                var expectedExceptionType = typeof(EntryPointNotFoundException);
                if (hostType != HostType.IIS)
                {
                    Assert.Throws(expectedExceptionType, () => deployer.Deploy<ConfigurationMethodNotFoundTest>(hostType));
                }
                else
                {
                    string applicationUrl = deployer.Deploy<ConfigurationMethodNotFoundTest>(hostType);
                    Assert.True(HttpClientUtility.GetResponseTextFromUrl(applicationUrl).Contains(expectedExceptionType.Name), "Fatal error not thrown without Configuration method");
                }
            }
        }
    }
}
