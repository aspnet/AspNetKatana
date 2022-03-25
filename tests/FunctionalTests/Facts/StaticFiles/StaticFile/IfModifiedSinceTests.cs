// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using FunctionalTests.Common;
using Owin;
using Xunit;
using Xunit.Extensions;

namespace FunctionalTests.Facts.StaticFiles
{
    public class IfModifiedSinceTests
    {
        [Theory, Trait("FunctionalTests", "General")]
        [InlineData(HostType.HttpListener)]
        public void Static_ValidIfModifiedSince(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                var applicationUrl = deployer.Deploy(hostType, ValidModifiedSinceConfiguration);
                var httpClient = new HttpClient() { BaseAddress = new Uri(applicationUrl) };
                var fileContent = File.ReadAllBytes(@"RequirementFiles/Dir1/RangeRequest.txt");

                var response = httpClient.GetAsync(@"RequirementFiles/Dir1/RangeRequest.txt").Result;
                Assert.Equal<HttpStatusCode>(HttpStatusCode.OK, response.StatusCode);

                //Modified since = lastmodified. Expect a 304
                httpClient.DefaultRequestHeaders.IfModifiedSince = response.Content.Headers.LastModified;
                response = httpClient.GetAsync(@"RequirementFiles/Dir1/RangeRequest.txt").Result;
                Assert.Equal<HttpStatusCode>(HttpStatusCode.NotModified, response.StatusCode);

                //Modified since > lastmodified. Expect a 304
                httpClient.DefaultRequestHeaders.IfModifiedSince = response.Content.Headers.LastModified.Value.AddMinutes(12);
                response = httpClient.GetAsync(@"RequirementFiles/Dir1/RangeRequest.txt").Result;
                Assert.Equal<HttpStatusCode>(HttpStatusCode.NotModified, response.StatusCode);

                //Modified since < lastmodified. Expect an OK. 
                httpClient.DefaultRequestHeaders.IfModifiedSince = response.Content.Headers.LastModified.Value.Subtract(new TimeSpan(10 * 1000));
                response = httpClient.GetAsync(@"RequirementFiles/Dir1/RangeRequest.txt").Result;
                Assert.Equal<HttpStatusCode>(HttpStatusCode.OK, response.StatusCode);
                CompareBytes(fileContent, response.Content.ReadAsByteArrayAsync().Result, 0, fileContent.Length - 1);

                //Modified since is an invalid date string. Expect an OK. 
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("If-Modified-Since", "InvalidDate");
                response = httpClient.GetAsync(@"RequirementFiles/Dir1/RangeRequest.txt").Result;
                Assert.Equal<HttpStatusCode>(HttpStatusCode.OK, response.StatusCode);
            }
        }

        internal void ValidModifiedSinceConfiguration(IAppBuilder app)
        {
            app.UseStaticFiles();
        }

        private void CompareBytes(byte[] fileContent, byte[] byteRange, long start, long end)
        {
            Assert.Equal<long>(end - start + 1, byteRange.Length);
            for (long i = start; i <= end; i++)
            {
                Assert.Equal<byte>(fileContent[i], byteRange[i - start]);
            }
        }
    }
}