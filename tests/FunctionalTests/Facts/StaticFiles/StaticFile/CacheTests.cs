// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using FunctionalTests.Common;
using Owin;
using Xunit;
using Xunit.Extensions;

namespace FunctionalTests.Facts.StaticFiles
{
    public class CacheTests
    {
        [Theory, Trait("FunctionalTests", "General")]
        [InlineData(HostType.HttpListener)]
        public void Static_CacheHeadersDefault(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                var applicationUrl = deployer.Deploy(hostType, CacheHeadersDefaultConfiguration);
                var httpClient = new HttpClient() { BaseAddress = new Uri(applicationUrl) };

                var response = httpClient.GetAsync("RequirementFiles/Dir1/Default.html").Result;
                Assert.True(!string.IsNullOrWhiteSpace(response.Headers.ETag.Tag), "E-Tag header missing");
                Assert.True(response.Headers.ETag.Tag.StartsWith("\""), "E-Tag header does not start with a quote");
                Assert.True(response.Headers.ETag.Tag.EndsWith("\""), "E-Tag header does not end with a quote");
                Assert.True(response.Content.Headers.LastModified.HasValue, "Date-Modified header missing");
            }
        }

        internal void CacheHeadersDefaultConfiguration(IAppBuilder app)
        {
            app.UseStaticFiles();
        }
    }
}