// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
    public class IfUnModifiedSinceTests
    {
        [Theory, Trait("FunctionalTests", "General")]
        [InlineData(HostType.HttpListener)]
        public void Static_ValidIfUnModifiedSince(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                var applicationUrl = deployer.Deploy(hostType, ValidUnModifiedSinceConfiguration);
                var httpClient = new HttpClient() { BaseAddress = new Uri(applicationUrl) };
                var fileContent = File.ReadAllBytes(@"RequirementFiles/Dir1/RangeRequest.txt");

                var response = httpClient.GetAsync(@"RequirementFiles/Dir1/RangeRequest.txt").Result;
                Assert.Equal<HttpStatusCode>(HttpStatusCode.OK, response.StatusCode);

                //Modified since = lastmodified. Expect a 304
                httpClient.DefaultRequestHeaders.IfUnmodifiedSince = response.Content.Headers.LastModified;
                response = httpClient.GetAsync(@"RequirementFiles/Dir1/RangeRequest.txt").Result;
                Assert.Equal<HttpStatusCode>(HttpStatusCode.OK, response.StatusCode);

                //Modified since > lastmodified. Expect a 304
                httpClient.DefaultRequestHeaders.IfUnmodifiedSince = response.Content.Headers.LastModified.Value.AddMinutes(12);
                response = httpClient.GetAsync(@"RequirementFiles/Dir1/RangeRequest.txt").Result;
                Assert.Equal<HttpStatusCode>(HttpStatusCode.OK, response.StatusCode);

                //Modified since < lastmodified. Expect an OK. 
                httpClient.DefaultRequestHeaders.IfUnmodifiedSince = response.Content.Headers.LastModified.Value.Subtract(new TimeSpan(10 * 1000));
                response = httpClient.GetAsync(@"RequirementFiles/Dir1/RangeRequest.txt").Result;
                Assert.Equal<HttpStatusCode>(HttpStatusCode.PreconditionFailed, response.StatusCode);

                //Modified since is an invalid date string. Expect an OK. 
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("If-Unmodified-Since", "InvalidDate");
                response = httpClient.GetAsync(@"RequirementFiles/Dir1/RangeRequest.txt").Result;
                Assert.Equal<HttpStatusCode>(HttpStatusCode.OK, response.StatusCode);
            }
        }

        public void ValidUnModifiedSinceConfiguration(IAppBuilder app)
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