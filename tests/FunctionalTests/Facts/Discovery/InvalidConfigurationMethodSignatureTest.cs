// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using FunctionalTests.Common;
using Xunit;
using Xunit.Extensions;

namespace FunctionalTests.Facts.Discovery
{
    public class InvalidConfigurationMethodSignatureTest
    {
        [Theory, Trait("FunctionalTests", "General")]
        [InlineData(HostType.IIS)]
        [InlineData(HostType.HttpListener)]
        public void InvalidConfigurationMethodSignature(HostType hostType)
        {
            var expectedExceptionType = typeof(EntryPointNotFoundException);
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                if (hostType != HostType.IIS)
                {
                    Assert.Throws(expectedExceptionType, () => deployer.Deploy<InvalidConfigurationMethodSignatureTest>(hostType));
                }
                else
                {
                    string applicationUrl = deployer.Deploy<InvalidConfigurationMethodSignatureTest>(hostType);
                    Assert.True(HttpClientUtility.GetResponseTextFromUrl(applicationUrl).Contains(expectedExceptionType.Name), "Fatal error not thrown with invalid Configuration method signature");
                }
            }
        }

        internal void Configuration(object app)
        {
        }
    }
}
