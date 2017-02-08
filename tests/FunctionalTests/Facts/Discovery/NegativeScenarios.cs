// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using FunctionalTests.Common;
using Owin;
using Xunit;
using Xunit.Extensions;

namespace FunctionalTests.Facts.Discovery
{
    public class NegativeScenarios
    {
        static Type expectedExceptionType = typeof(EntryPointNotFoundException);

        [Theory, Trait("FunctionalTests", "General")]
        [InlineData(HostType.IIS)]
        public void AppStartupNotResolvable(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                string applicationUrl = deployer.Deploy(hostType, ValidConfiguration);

                string webConfig = deployer.GetWebConfigPath();
                string fullyQualifiedConfigurationMethodName = ((Action<IAppBuilder>)ValidConfiguration).GetFullyQualifiedConfigurationMethodName();
                string webConfigContent = File.ReadAllText(webConfig).
                    Replace(fullyQualifiedConfigurationMethodName, "NotResolvableStartup");
                File.WriteAllText(webConfig, webConfigContent);

                Assert.True(HttpClientUtility.GetResponseTextFromUrl(applicationUrl).Contains(expectedExceptionType.Name), "Fatal error not thrown with a not resolvable startup");
            }
        }

        [Theory, Trait("FunctionalTests", "General")]
        [InlineData(HostType.IIS)]
        public void InvalidAppStartupInConfiguration(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                string applicationUrl = deployer.Deploy(hostType, ValidConfiguration);
                string webConfig = deployer.GetWebConfigPath();
                string webConfigContent = File.ReadAllText(webConfig).Replace(typeof(NegativeScenarios).Name, "NotExistingStartupClass");
                File.WriteAllText(webConfig, webConfigContent);
                Assert.True(HttpClientUtility.GetResponseTextFromUrl(applicationUrl).Contains(expectedExceptionType.Name), "Fatal error not thrown with invalid owin:AppStartup");
            }
        }

        [Theory, Trait("FunctionalTests", "General")]
        [InlineData(HostType.IIS)]
        public void InvalidAssemblyNameInConfiguration(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                string applicationUrl = deployer.Deploy(hostType, ValidConfiguration);

                string webConfig = deployer.GetWebConfigPath();
                string fullyQualifiedConfigurationMethodName = ((Action<IAppBuilder>)ValidConfiguration).GetFullyQualifiedConfigurationMethodName();
                string webConfigContent = File.ReadAllText(webConfig).
                    Replace(fullyQualifiedConfigurationMethodName, fullyQualifiedConfigurationMethodName + ", NotExistingAssembly");
                File.WriteAllText(webConfig, webConfigContent);

                Assert.True(HttpClientUtility.GetResponseTextFromUrl(applicationUrl).Contains(expectedExceptionType.Name), "Fatal error not thrown with invalid assembly name");
            }
        }

        public void ValidConfiguration(IAppBuilder app)
        {
        }
    }
}
