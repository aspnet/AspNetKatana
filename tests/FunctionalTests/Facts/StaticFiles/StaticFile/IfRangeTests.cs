// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using FunctionalTests.Common;
using Owin;
using Xunit;
using Xunit.Extensions;

namespace FunctionalTests.Facts.StaticFiles
{
    public class IfRangeTests
    {
        [Theory, Trait("FunctionalTests", "General")]
        [InlineData(HostType.HttpListener)]
        public void Static_ValidIfRange(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                var applicationUrl = deployer.Deploy(hostType, ValidIfRangeConfiguration);
                var httpClient = new HttpClient() { BaseAddress = new Uri(applicationUrl) };

                var fileContent = File.ReadAllBytes(@"RequirementFiles/Dir1/RangeRequest.txt");

                var response = httpClient.GetAsync(@"RequirementFiles/Dir1/RangeRequest.txt").Result;
                Assert.Equal<HttpStatusCode>(HttpStatusCode.OK, response.StatusCode);

                //Range with 500 bytes starting at 0 with a conditional Etag. 
                httpClient.DefaultRequestHeaders.Range = new RangeHeaderValue(0, 499);
                httpClient.DefaultRequestHeaders.IfRange = new RangeConditionHeaderValue(response.Headers.ETag.Tag);
                response = httpClient.GetAsync(@"RequirementFiles/Dir1/RangeRequest.txt").Result;
                Assert.Equal<HttpStatusCode>(HttpStatusCode.PartialContent, response.StatusCode);
                Assert.True(response.Content.Headers.ContentRange.HasRange &&
                    response.Content.Headers.ContentRange.Unit == "bytes" && response.Content.Headers.ContentRange.Length == fileContent.Length &&
                    response.Content.Headers.ContentRange.From == 0 && response.Content.Headers.ContentRange.To == 499);
                Assert.Equal<long?>(500, response.Content.Headers.ContentLength);
                CompareBytes(fileContent, response.Content.ReadAsByteArrayAsync().Result, 0, 499);

                //Range with 500 bytes starting at 0 with an invalid Etag - Range header is ignored if If-Range evalutates to false.
                httpClient.DefaultRequestHeaders.Range = new RangeHeaderValue(0, 499);
                httpClient.DefaultRequestHeaders.IfRange = new RangeConditionHeaderValue("\"invalidEtag\"");
                response = httpClient.GetAsync(@"RequirementFiles/Dir1/RangeRequest.txt").Result;
                Assert.Equal<HttpStatusCode>(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal<long?>(fileContent.Length, response.Content.Headers.ContentLength);
                CompareBytes(fileContent, response.Content.ReadAsByteArrayAsync().Result, 0, fileContent.Length - 1);

                //Range with 500 bytes starting at 0 with a If-Range date = lastmodified
                httpClient.DefaultRequestHeaders.Range = new RangeHeaderValue(0, 499);
                httpClient.DefaultRequestHeaders.IfRange = new RangeConditionHeaderValue(response.Content.Headers.LastModified.Value);
                response = httpClient.GetAsync(@"RequirementFiles/Dir1/RangeRequest.txt").Result;
                Assert.Equal<HttpStatusCode>(HttpStatusCode.PartialContent, response.StatusCode);
                Assert.True(response.Content.Headers.ContentRange.HasRange &&
                    response.Content.Headers.ContentRange.Unit == "bytes" && response.Content.Headers.ContentRange.Length == fileContent.Length &&
                    response.Content.Headers.ContentRange.From == 0 && response.Content.Headers.ContentRange.To == 499);
                Assert.Equal<long?>(500, response.Content.Headers.ContentLength);
                CompareBytes(fileContent, response.Content.ReadAsByteArrayAsync().Result, 0, 499);

                //Range with 500 bytes starting at 0 with a If-Range date > Lastmodified
                httpClient.DefaultRequestHeaders.Range = new RangeHeaderValue(0, 499);
                httpClient.DefaultRequestHeaders.IfRange = new RangeConditionHeaderValue(response.Content.Headers.LastModified.Value.Add(new TimeSpan(10 * 1000)));
                response = httpClient.GetAsync(@"RequirementFiles/Dir1/RangeRequest.txt").Result;
                Assert.Equal<HttpStatusCode>(HttpStatusCode.PartialContent, response.StatusCode);
                Assert.True(response.Content.Headers.ContentRange.HasRange &&
                    response.Content.Headers.ContentRange.Unit == "bytes" && response.Content.Headers.ContentRange.Length == fileContent.Length &&
                    response.Content.Headers.ContentRange.From == 0 && response.Content.Headers.ContentRange.To == 499);
                Assert.Equal<long?>(500, response.Content.Headers.ContentLength);
                CompareBytes(fileContent, response.Content.ReadAsByteArrayAsync().Result, 0, 499);

                //Range with 500 bytes starting at 0 with a If-Range date < lastmodified
                httpClient.DefaultRequestHeaders.Range = new RangeHeaderValue(0, 499);
                httpClient.DefaultRequestHeaders.IfRange = new RangeConditionHeaderValue(response.Content.Headers.LastModified.Value.Subtract(new TimeSpan(10 * 1000)));
                response = httpClient.GetAsync(@"RequirementFiles/Dir1/RangeRequest.txt").Result;
                Assert.Equal<HttpStatusCode>(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal<long?>(fileContent.Length, response.Content.Headers.ContentLength);
                CompareBytes(fileContent, response.Content.ReadAsByteArrayAsync().Result, 0, fileContent.Length - 1);

                //Range with 500 bytes starting at 0 with a If-Range date which is invalid
                httpClient.DefaultRequestHeaders.Range = new RangeHeaderValue(0, 499);
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("If-Range", "InvalidDate");
                response = httpClient.GetAsync(@"RequirementFiles/Dir1/RangeRequest.txt").Result;
                Assert.Equal<HttpStatusCode>(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal<long?>(fileContent.Length, response.Content.Headers.ContentLength);
                CompareBytes(fileContent, response.Content.ReadAsByteArrayAsync().Result, 0, fileContent.Length - 1);

                //-0. Range is not satisfiable, but the If-Range evaluated to partial content, then we expect full content. 
                httpClient.DefaultRequestHeaders.Range = new RangeHeaderValue(null, 0);
                httpClient.DefaultRequestHeaders.IfRange = new RangeConditionHeaderValue(response.Content.Headers.LastModified.Value.Add(new TimeSpan(10 * 1000)));
                response = httpClient.GetAsync(@"RequirementFiles/Dir1/RangeRequest.txt").Result;
                Assert.Equal<HttpStatusCode>(HttpStatusCode.RequestedRangeNotSatisfiable, response.StatusCode);
                Assert.Equal<long?>(0, response.Content.Headers.ContentLength);
                Assert.Equal("bytes */2543", response.Content.Headers.ContentRange.ToString());
            }
        }

        internal void ValidIfRangeConfiguration(IAppBuilder app)
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
