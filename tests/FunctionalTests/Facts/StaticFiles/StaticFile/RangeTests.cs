// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using FunctionalTests.Common;
using Microsoft.Owin.Testing;
using Owin;
using Xunit;
using Xunit.Extensions;

namespace FunctionalTests.Facts.StaticFiles
{
    public class RangeTests
    {
        [Theory, Trait("FunctionalTests", "General")]
        [InlineData(HostType.HttpListener)]
        public void Static_ValidRange(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                var applicationUrl = deployer.Deploy(hostType, ValidRangeConfiguration);
                var httpClient = new HttpClient() { BaseAddress = new Uri(applicationUrl) };
                var fileContent = File.ReadAllBytes(@"RequirementFiles/Dir1/RangeRequest.txt");

                //Range with 1 bytes starting at 0
                httpClient.DefaultRequestHeaders.Range = new RangeHeaderValue(0, 0);
                var response = httpClient.GetAsync(@"RequirementFiles/Dir1/RangeRequest.txt").Result;
                Assert.Equal<HttpStatusCode>(HttpStatusCode.PartialContent, response.StatusCode);
                Assert.True(response.Content.Headers.ContentRange.HasRange &&
                    response.Content.Headers.ContentRange.Unit == "bytes" && response.Content.Headers.ContentRange.Length == fileContent.Length &&
                    response.Content.Headers.ContentRange.From == 0 && response.Content.Headers.ContentRange.To == 0);
                Assert.Equal<long?>(1, response.Content.Headers.ContentLength);
                CompareBytes(fileContent, response.Content.ReadAsByteArrayAsync().Result, 0, 0);

                //Range with 1 bytes starting at 9
                httpClient.DefaultRequestHeaders.Range = new RangeHeaderValue(9, 9);
                response = httpClient.GetAsync(@"RequirementFiles/Dir1/RangeRequest.txt").Result;
                Assert.Equal<HttpStatusCode>(HttpStatusCode.PartialContent, response.StatusCode);
                Assert.True(response.Content.Headers.ContentRange.HasRange &&
                    response.Content.Headers.ContentRange.Unit == "bytes" && response.Content.Headers.ContentRange.Length == fileContent.Length &&
                    response.Content.Headers.ContentRange.From == 9 && response.Content.Headers.ContentRange.To == 9);
                Assert.Equal<long?>(1, response.Content.Headers.ContentLength);
                CompareBytes(fileContent, response.Content.ReadAsByteArrayAsync().Result, 9, 9);

                //Range with 2 bytes starting at 5
                httpClient.DefaultRequestHeaders.Range = new RangeHeaderValue(5, 6);
                response = httpClient.GetAsync(@"RequirementFiles/Dir1/RangeRequest.txt").Result;
                Assert.Equal<HttpStatusCode>(HttpStatusCode.PartialContent, response.StatusCode);
                Assert.True(response.Content.Headers.ContentRange.HasRange &&
                    response.Content.Headers.ContentRange.Unit == "bytes" && response.Content.Headers.ContentRange.Length == fileContent.Length &&
                    response.Content.Headers.ContentRange.From == 5 && response.Content.Headers.ContentRange.To == 6);
                Assert.Equal<long?>(2, response.Content.Headers.ContentLength);
                CompareBytes(fileContent, response.Content.ReadAsByteArrayAsync().Result, 5, 6);

                //Range with 500 bytes starting at 0
                httpClient.DefaultRequestHeaders.Range = new RangeHeaderValue(0, 499);
                response = httpClient.GetAsync(@"RequirementFiles/Dir1/RangeRequest.txt").Result;
                Assert.Equal<HttpStatusCode>(HttpStatusCode.PartialContent, response.StatusCode);
                Assert.True(response.Content.Headers.ContentRange.HasRange &&
                    response.Content.Headers.ContentRange.Unit == "bytes" && response.Content.Headers.ContentRange.Length == fileContent.Length &&
                    response.Content.Headers.ContentRange.From == 0 && response.Content.Headers.ContentRange.To == 499);
                Assert.Equal<long?>(500, response.Content.Headers.ContentLength);
                CompareBytes(fileContent, response.Content.ReadAsByteArrayAsync().Result, 0, 499);

                //Range with second 500 bytes starting at 500
                httpClient.DefaultRequestHeaders.Range = new RangeHeaderValue(500, 999);
                response = httpClient.GetAsync(@"RequirementFiles/Dir1/RangeRequest.txt").Result;
                Assert.Equal<HttpStatusCode>(HttpStatusCode.PartialContent, response.StatusCode);
                Assert.True(response.Content.Headers.ContentRange.HasRange &&
                    response.Content.Headers.ContentRange.Unit == "bytes" && response.Content.Headers.ContentRange.Length == fileContent.Length &&
                    response.Content.Headers.ContentRange.From == 500 && response.Content.Headers.ContentRange.To == 999);
                Assert.Equal<long?>(500, response.Content.Headers.ContentLength);
                CompareBytes(fileContent, response.Content.ReadAsByteArrayAsync().Result, 500, 999);

                //-500
                httpClient.DefaultRequestHeaders.Range = new RangeHeaderValue(null, 500);
                response = httpClient.GetAsync(@"RequirementFiles/Dir1/RangeRequest.txt").Result;
                Assert.Equal<HttpStatusCode>(HttpStatusCode.PartialContent, response.StatusCode);
                Assert.True(response.Content.Headers.ContentRange.HasRange &&
                    response.Content.Headers.ContentRange.Unit == "bytes" && response.Content.Headers.ContentRange.Length == fileContent.Length &&
                    response.Content.Headers.ContentRange.From == fileContent.Length - 500 && response.Content.Headers.ContentRange.To == fileContent.Length - 1);
                Assert.Equal<long?>(500, response.Content.Headers.ContentLength);
                CompareBytes(fileContent, response.Content.ReadAsByteArrayAsync().Result, fileContent.Length - 500, fileContent.Length - 1);

                //0-
                httpClient.DefaultRequestHeaders.Range = new RangeHeaderValue(0, null);
                response = httpClient.GetAsync(@"RequirementFiles/Dir1/RangeRequest.txt").Result;
                Assert.Equal<HttpStatusCode>(HttpStatusCode.PartialContent, response.StatusCode);
                Assert.True(response.Content.Headers.ContentRange.HasRange &&
                    response.Content.Headers.ContentRange.Unit == "bytes" && response.Content.Headers.ContentRange.Length == fileContent.Length &&
                    response.Content.Headers.ContentRange.From == 0 && response.Content.Headers.ContentRange.To == fileContent.Length - 1);
                Assert.Equal<long?>(fileContent.Length, response.Content.Headers.ContentLength);
                CompareBytes(fileContent, response.Content.ReadAsByteArrayAsync().Result, 0, fileContent.Length - 1);

                //10-
                httpClient.DefaultRequestHeaders.Range = new RangeHeaderValue(10, null);
                response = httpClient.GetAsync(@"RequirementFiles/Dir1/RangeRequest.txt").Result;
                Assert.Equal<HttpStatusCode>(HttpStatusCode.PartialContent, response.StatusCode);
                Assert.True(response.Content.Headers.ContentRange.HasRange &&
                    response.Content.Headers.ContentRange.Unit == "bytes" && response.Content.Headers.ContentRange.Length == fileContent.Length &&
                    response.Content.Headers.ContentRange.From == 10 && response.Content.Headers.ContentRange.To == fileContent.Length - 1);
                Assert.Equal<long?>(fileContent.Length - 10, response.Content.Headers.ContentLength);
                CompareBytes(fileContent, response.Content.ReadAsByteArrayAsync().Result, 10, fileContent.Length - 1);

                //0-10*10000
                httpClient.DefaultRequestHeaders.Range = new RangeHeaderValue(0, 10 * 10000);
                response = httpClient.GetAsync(@"RequirementFiles/Dir1/RangeRequest.txt").Result;
                Assert.Equal<HttpStatusCode>(HttpStatusCode.PartialContent, response.StatusCode);
                Assert.True(response.Content.Headers.ContentRange.HasRange &&
                    response.Content.Headers.ContentRange.Unit == "bytes" && response.Content.Headers.ContentRange.Length == fileContent.Length &&
                    response.Content.Headers.ContentRange.From == 0 && response.Content.Headers.ContentRange.To == fileContent.Length - 1);
                Assert.Equal<long?>(fileContent.Length, response.Content.Headers.ContentLength);
                CompareBytes(fileContent, response.Content.ReadAsByteArrayAsync().Result, 0, fileContent.Length - 1);

                //9500-
                httpClient.DefaultRequestHeaders.Range = new RangeHeaderValue(9500, null);
                response = httpClient.GetAsync(@"RequirementFiles/Dir1/RangeRequest.txt").Result;
                Assert.Equal<HttpStatusCode>(HttpStatusCode.RequestedRangeNotSatisfiable, response.StatusCode);
                Assert.Equal<long?>(0, response.Content.Headers.ContentLength);

                //-0
                httpClient.DefaultRequestHeaders.Range = new RangeHeaderValue(null, 0);
                response = httpClient.GetAsync(@"RequirementFiles/Dir1/RangeRequest.txt").Result;
                Assert.Equal<HttpStatusCode>(HttpStatusCode.RequestedRangeNotSatisfiable, response.StatusCode);
                Assert.Equal<long?>(0, response.Content.Headers.ContentLength);

                //Invalid ranges last-post > first-pos
                httpClient.DefaultRequestHeaders.Range = null;
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Range", "bytes=100-50");
                response = httpClient.GetAsync(@"RequirementFiles/Dir1/RangeRequest.txt").Result;
                Assert.Equal<HttpStatusCode>(HttpStatusCode.OK, response.StatusCode);
                Assert.Null(response.Content.Headers.ContentRange);
                Assert.Equal<long>(fileContent.Length, response.Content.Headers.ContentLength.Value);

                //Invalid Customized ranges 'pages' instead of 'bytes'. We don't understand what this is. So we should return the full page. 
                httpClient.DefaultRequestHeaders.Remove("Range");
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Range", "pages=1-b");
                response = httpClient.GetAsync(@"RequirementFiles/Dir1/RangeRequest.txt").Result;
                Assert.Equal<HttpStatusCode>(HttpStatusCode.OK, response.StatusCode);
                Assert.Null(response.Content.Headers.ContentRange);
                Assert.Equal<long>(fileContent.Length, response.Content.Headers.ContentLength.Value);

                //Multi range requests - not understand we serve the entire file
                httpClient.DefaultRequestHeaders.Range = new RangeHeaderValue();
                httpClient.DefaultRequestHeaders.Range.Ranges.Add(new RangeItemHeaderValue(1, 2));
                httpClient.DefaultRequestHeaders.Range.Ranges.Add(new RangeItemHeaderValue(5, 12));
                httpClient.DefaultRequestHeaders.Range.Ranges.Add(new RangeItemHeaderValue(22, 32));
                response = httpClient.GetAsync(@"RequirementFiles/Dir1/RangeRequest.txt").Result;
                Assert.Equal<HttpStatusCode>(HttpStatusCode.OK, response.StatusCode);
                Assert.Null(response.Content.Headers.ContentRange);
                Assert.Equal<long?>(fileContent.Length, response.Content.Headers.ContentLength);
                CompareBytes(fileContent, response.Content.ReadAsByteArrayAsync().Result, 0, fileContent.Length - 1);
            }
        }

        [Fact]
        public void Static_InValidRange()
        {
            using (var server = TestServer.Create(ValidRangeConfiguration))
            {
                var fileContent = File.ReadAllBytes(@"RequirementFiles/Dir1/RangeRequest.txt");

                //Invalid ranges invalid pos values
                var response = server.CreateRequest(@"RequirementFiles/Dir1/RangeRequest.txt").AddHeader("Range", "bytes=1-b").GetAsync().Result;
                Assert.Equal<HttpStatusCode>(HttpStatusCode.OK, response.StatusCode);
                Assert.Null(response.Content.Headers.ContentRange);
                Assert.Equal<long>(fileContent.Length, response.Content.Headers.ContentLength.Value);

                //Invalid ranges invalid pos values
                response = server.CreateRequest(@"RequirementFiles/Dir1/RangeRequest.txt").AddHeader("Range", "bytes=*-*").GetAsync().Result;
                Assert.Equal<HttpStatusCode>(HttpStatusCode.OK, response.StatusCode);
                Assert.Null(response.Content.Headers.ContentRange);
                Assert.Equal<long>(fileContent.Length, response.Content.Headers.ContentLength.Value);

                //Invalid ranges invalid pos values
                response = server.CreateRequest(@"RequirementFiles/Dir1/RangeRequest.txt").AddHeader("Range", "bytes=111111111111111111111111111111111111111111111111111132123111111111111111111111111111111111111111111111111111132123-111111111111111111111111111111111111111111111111111132123111111111111111111111111111111111111111111111111111132123").GetAsync().Result;
                Assert.Equal<HttpStatusCode>(HttpStatusCode.OK, response.StatusCode);
                Assert.Null(response.Content.Headers.ContentRange);
                Assert.Equal<long>(fileContent.Length, response.Content.Headers.ContentLength.Value);

                //Invalid ranges invalid pos values
                response = server.CreateRequest(@"RequirementFiles/Dir1/RangeRequest.txt").AddHeader("Range", "bytes=-").GetAsync().Result;
                Assert.Equal<HttpStatusCode>(HttpStatusCode.OK, response.StatusCode);
                Assert.Null(response.Content.Headers.ContentRange);
                Assert.Equal<long>(fileContent.Length, response.Content.Headers.ContentLength.Value);

                //Invalid ranges invalid pos values
                response = server.CreateRequest(@"RequirementFiles/Dir1/RangeRequest.txt").AddHeader("Range", "bytes=-,-,13-23").GetAsync().Result;
                Assert.Equal<HttpStatusCode>(HttpStatusCode.OK, response.StatusCode);
                Assert.Null(response.Content.Headers.ContentRange);
                Assert.Equal<long>(fileContent.Length, response.Content.Headers.ContentLength.Value);
            }
        }

        private void CompareBytes(byte[] fileContent, byte[] byteRange, long start, long end)
        {
            Assert.Equal<long>(end - start + 1, byteRange.Length);
            for (long i = start; i <= end; i++)
            {
                Assert.Equal<byte>(fileContent[i], byteRange[i - start]);
            }
        }

        internal void ValidRangeConfiguration(IAppBuilder app)
        {
            app.UseStaticFiles();
        }
    }
}