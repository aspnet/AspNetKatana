// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Owin.Testing;
using Owin;
using Xunit;
using Xunit.Extensions;

namespace Microsoft.Owin.StaticFiles.Tests
{
    public class RangeHeaderTests
    {
        // 14.27 If-Range
        // If the entity tag given in the If-Range header matches the current entity tag for the entity, then the server SHOULD 
        // provide the specified sub-range of the entity using a 206 (Partial content) response.
        [Fact]
        public async Task IfRangeWithCurrentEtagShouldServePartialContent()
        {
            TestServer server = TestServer.Create(app => app.UseFileServer());
            HttpResponseMessage original = await server.HttpClient.GetAsync("http://localhost/SubFolder/Ranges.txt");

            var req = new HttpRequestMessage(HttpMethod.Get, "http://localhost/SubFolder/Ranges.txt");
            req.Headers.Add("If-Range", original.Headers.ETag.ToString());
            req.Headers.Add("Range", "bytes=0-10");
            HttpResponseMessage resp = await server.HttpClient.SendAsync(req);
            Assert.Equal(HttpStatusCode.PartialContent, resp.StatusCode);
            Assert.Equal("bytes 0-10/62", resp.Content.Headers.ContentRange.ToString());
            Assert.Equal(11, resp.Content.Headers.ContentLength);
            Assert.Equal("0123456789a", await resp.Content.ReadAsStringAsync());
        }

        // 14.27 If-Range
        // If the entity tag given in the If-Range header matches the current entity tag for the entity, then the server SHOULD
        // provide the specified sub-range of the entity using a 206 (Partial content) response.
        [Fact]
        public async Task HEADIfRangeWithCurrentEtagShouldServePartialContent()
        {
            TestServer server = TestServer.Create(app => app.UseFileServer());
            HttpResponseMessage original = await server.HttpClient.GetAsync("http://localhost/SubFolder/Ranges.txt");

            var req = new HttpRequestMessage(HttpMethod.Head, "http://localhost/SubFolder/Ranges.txt");
            req.Headers.Add("If-Range", original.Headers.ETag.ToString());
            req.Headers.Add("Range", "bytes=0-10");
            HttpResponseMessage resp = await server.HttpClient.SendAsync(req);
            Assert.Equal(HttpStatusCode.PartialContent, resp.StatusCode);
            Assert.Equal("bytes 0-10/62", resp.Content.Headers.ContentRange.ToString());
            Assert.Equal(11, resp.Content.Headers.ContentLength);
            Assert.Equal(string.Empty, await resp.Content.ReadAsStringAsync());
        }

        // 14.27 If-Range
        // If the client has no entity tag for an entity, but does have a Last- Modified date, it MAY use that date in an If-Range header.
        [Fact]
        public async Task IfRangeWithCurrentDateShouldServePartialContent()
        {
            TestServer server = TestServer.Create(app => app.UseFileServer());
            HttpResponseMessage original = await server.HttpClient.GetAsync("http://localhost/SubFolder/Ranges.txt");

            var req = new HttpRequestMessage(HttpMethod.Get, "http://localhost/SubFolder/Ranges.txt");
            req.Headers.Add("If-Range", original.Content.Headers.LastModified.Value.ToString("r"));
            req.Headers.Add("Range", "bytes=0-10");
            HttpResponseMessage resp = await server.HttpClient.SendAsync(req);
            Assert.Equal(HttpStatusCode.PartialContent, resp.StatusCode);
            Assert.Equal("bytes 0-10/62", resp.Content.Headers.ContentRange.ToString());
            Assert.Equal(11, resp.Content.Headers.ContentLength);
            Assert.Equal("0123456789a", await resp.Content.ReadAsStringAsync());
        }

        // 14.27 If-Range
        // If the client has no entity tag for an entity, but does have a Last- Modified date, it MAY use that date in an If-Range header.
        [Fact]
        public async Task HEADIfRangeWithCurrentDateShouldServePartialContent()
        {
            TestServer server = TestServer.Create(app => app.UseFileServer());
            HttpResponseMessage original = await server.HttpClient.GetAsync("http://localhost/SubFolder/Ranges.txt");

            var req = new HttpRequestMessage(HttpMethod.Head, "http://localhost/SubFolder/Ranges.txt");
            req.Headers.Add("If-Range", original.Content.Headers.LastModified.Value.ToString("r"));
            req.Headers.Add("Range", "bytes=0-10");
            HttpResponseMessage resp = await server.HttpClient.SendAsync(req);
            Assert.Equal(HttpStatusCode.PartialContent, resp.StatusCode);
            Assert.Equal("bytes 0-10/62", resp.Content.Headers.ContentRange.ToString());
            Assert.Equal(11, resp.Content.Headers.ContentLength);
            Assert.Equal(string.Empty, await resp.Content.ReadAsStringAsync());
        }

        // 14.27 If-Range
        // If the entity tag does not match, then the server SHOULD return the entire entity using a 200 (OK) response.
        [Fact]
        public async Task IfRangeWithOldEtagShouldServeFullContent()
        {
            TestServer server = TestServer.Create(app => app.UseFileServer());
            var req = new HttpRequestMessage(HttpMethod.Get, "http://localhost/SubFolder/Ranges.txt");
            req.Headers.Add("If-Range", "\"OldEtag\"");
            req.Headers.Add("Range", "bytes=0-10");
            HttpResponseMessage resp = await server.HttpClient.SendAsync(req);
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
            Assert.Null(resp.Content.Headers.ContentRange);
            Assert.Equal(62, resp.Content.Headers.ContentLength);
            Assert.Equal("0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ", await resp.Content.ReadAsStringAsync());
        }

        // 14.27 If-Range
        // If the entity tag does not match, then the server SHOULD return the entire entity using a 200 (OK) response.
        [Fact]
        public async Task HEADIfRangeWithOldEtagShouldServeFullContent()
        {
            TestServer server = TestServer.Create(app => app.UseFileServer());
            var req = new HttpRequestMessage(HttpMethod.Head, "http://localhost/SubFolder/Ranges.txt");
            req.Headers.Add("If-Range", "\"OldEtag\"");
            req.Headers.Add("Range", "bytes=0-10");
            HttpResponseMessage resp = await server.HttpClient.SendAsync(req);
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
            Assert.Null(resp.Content.Headers.ContentRange);
            Assert.Equal(62, resp.Content.Headers.ContentLength);
            Assert.Equal(string.Empty, await resp.Content.ReadAsStringAsync());
        }

        // 14.27 If-Range
        // If the entity tag/date does not match, then the server SHOULD return the entire entity using a 200 (OK) response.
        [Fact]
        public async Task IfRangeWithOldDateShouldServeFullContent()
        {
            TestServer server = TestServer.Create(app => app.UseFileServer());
            HttpResponseMessage original = await server.HttpClient.GetAsync("http://localhost/SubFolder/Ranges.txt");

            var req = new HttpRequestMessage(HttpMethod.Get, "http://localhost/SubFolder/Ranges.txt");
            req.Headers.Add("If-Range", original.Content.Headers.LastModified.Value.Subtract(TimeSpan.FromDays(1)).ToString("r"));
            req.Headers.Add("Range", "bytes=0-10");
            HttpResponseMessage resp = await server.HttpClient.SendAsync(req);
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
            Assert.Null(resp.Content.Headers.ContentRange);
            Assert.Equal(62, resp.Content.Headers.ContentLength);
            Assert.Equal("0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ", await resp.Content.ReadAsStringAsync());
        }

        // 14.27 If-Range
        // If the entity tag/date does not match, then the server SHOULD return the entire entity using a 200 (OK) response.
        [Fact]
        public async Task HEADIfRangeWithOldDateShouldServeFullContent()
        {
            TestServer server = TestServer.Create(app => app.UseFileServer());
            HttpResponseMessage original = await server.HttpClient.GetAsync("http://localhost/SubFolder/Ranges.txt");

            var req = new HttpRequestMessage(HttpMethod.Head, "http://localhost/SubFolder/Ranges.txt");
            req.Headers.Add("If-Range", original.Content.Headers.LastModified.Value.Subtract(TimeSpan.FromDays(1)).ToString("r"));
            req.Headers.Add("Range", "bytes=0-10");
            HttpResponseMessage resp = await server.HttpClient.SendAsync(req);
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
            Assert.Null(resp.Content.Headers.ContentRange);
            Assert.Equal(62, resp.Content.Headers.ContentLength);
            Assert.Equal(string.Empty, await resp.Content.ReadAsStringAsync());
        }

        // 14.27 If-Range
        // The If-Range header SHOULD only be used together with a Range header, and MUST be ignored if the request 
        // does not include a Range header, or if the server does not support the sub-range operation.
        [Fact]
        public async Task IfRangeWithoutRangeShouldServeFullContent()
        {
            TestServer server = TestServer.Create(app => app.UseFileServer());
            HttpResponseMessage original = await server.HttpClient.GetAsync("http://localhost/SubFolder/Ranges.txt");

            var req = new HttpRequestMessage(HttpMethod.Get, "http://localhost/SubFolder/Ranges.txt");
            req.Headers.Add("If-Range", original.Headers.ETag.ToString());
            HttpResponseMessage resp = await server.HttpClient.SendAsync(req);
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
            Assert.Null(resp.Content.Headers.ContentRange);
            Assert.Equal(62, resp.Content.Headers.ContentLength);
            Assert.Equal("0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ", await resp.Content.ReadAsStringAsync());

            req = new HttpRequestMessage(HttpMethod.Get, "http://localhost/SubFolder/Ranges.txt");
            req.Headers.Add("If-Range", original.Content.Headers.LastModified.Value.ToString("r"));
            resp = await server.HttpClient.SendAsync(req);
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
            Assert.Null(resp.Content.Headers.ContentRange);
            Assert.Equal(62, resp.Content.Headers.ContentLength);
            Assert.Equal("0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ", await resp.Content.ReadAsStringAsync());
        }

        // 14.27 If-Range
        // The If-Range header SHOULD only be used together with a Range header, and MUST be ignored if the request
        // does not include a Range header, or if the server does not support the sub-range operation.
        [Fact]
        public async Task HEADIfRangeWithoutRangeShouldServeFullContent()
        {
            TestServer server = TestServer.Create(app => app.UseFileServer());
            HttpResponseMessage original = await server.HttpClient.GetAsync("http://localhost/SubFolder/Ranges.txt");

            var req = new HttpRequestMessage(HttpMethod.Head, "http://localhost/SubFolder/Ranges.txt");
            req.Headers.Add("If-Range", original.Headers.ETag.ToString());
            HttpResponseMessage resp = await server.HttpClient.SendAsync(req);
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
            Assert.Null(resp.Content.Headers.ContentRange);
            Assert.Equal(62, resp.Content.Headers.ContentLength);
            Assert.Equal(string.Empty, await resp.Content.ReadAsStringAsync());

            req = new HttpRequestMessage(HttpMethod.Head, "http://localhost/SubFolder/Ranges.txt");
            req.Headers.Add("If-Range", original.Content.Headers.LastModified.Value.ToString("r"));
            resp = await server.HttpClient.SendAsync(req);
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
            Assert.Null(resp.Content.Headers.ContentRange);
            Assert.Equal(62, resp.Content.Headers.ContentLength);
            Assert.Equal(string.Empty, await resp.Content.ReadAsStringAsync());
        }

        // 14.35 Range
        [Theory]
        [InlineData("0-0", "0-0", 1, "0")]
        [InlineData("0-9", "0-9", 10, "0123456789")]
        [InlineData("10-35", "10-35", 26, "abcdefghijklmnopqrstuvwxyz")]
        [InlineData("36-61", "36-61", 26, "ABCDEFGHIJKLMNOPQRSTUVWXYZ")]
        [InlineData("36-", "36-61", 26, "ABCDEFGHIJKLMNOPQRSTUVWXYZ")] // Last 26
        [InlineData("-26", "36-61", 26, "ABCDEFGHIJKLMNOPQRSTUVWXYZ")] // Last 26
        [InlineData("0-", "0-61", 62, "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ")]
        [InlineData("-1001", "0-61", 62, "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ")]
        public async Task SingleValidRangeShouldServePartialContent(string range, string expectedRange, int length, string expectedData)
        {
            TestServer server = TestServer.Create(app => app.UseFileServer());
            var req = new HttpRequestMessage(HttpMethod.Get, "http://localhost/SubFolder/Ranges.txt");
            req.Headers.Add("Range", "bytes=" + range);
            HttpResponseMessage resp = await server.HttpClient.SendAsync(req);
            Assert.Equal(HttpStatusCode.PartialContent, resp.StatusCode);
            Assert.NotNull(resp.Content.Headers.ContentRange);
            Assert.Equal("bytes " + expectedRange + "/62", resp.Content.Headers.ContentRange.ToString());
            Assert.Equal(length, resp.Content.Headers.ContentLength);
            Assert.Equal(expectedData,  await resp.Content.ReadAsStringAsync());
        }

        // 14.35 Range
        [Theory]
        [InlineData("0-0", "0-0", 1)]
        [InlineData("0-9", "0-9", 10)]
        [InlineData("10-35", "10-35", 26)]
        [InlineData("36-61", "36-61", 26)]
        [InlineData("36-", "36-61", 26)] // Last 26
        [InlineData("-26", "36-61", 26)] // Last 26
        [InlineData("0-", "0-61", 62)]
        [InlineData("-1001", "0-61", 62)]
        public async Task HEADSingleValidRangeShouldServePartialContent(string range, string expectedRange, int length)
        {
            TestServer server = TestServer.Create(app => app.UseFileServer());
            var req = new HttpRequestMessage(HttpMethod.Head, "http://localhost/SubFolder/Ranges.txt");
            req.Headers.Add("Range", "bytes=" + range);
            HttpResponseMessage resp = await server.HttpClient.SendAsync(req);
            Assert.Equal(HttpStatusCode.PartialContent, resp.StatusCode);
            Assert.NotNull(resp.Content.Headers.ContentRange);
            Assert.Equal("bytes " + expectedRange + "/62", resp.Content.Headers.ContentRange.ToString());
            Assert.Equal(length, resp.Content.Headers.ContentLength);
            Assert.Equal(string.Empty, await resp.Content.ReadAsStringAsync());
        }

        // 14.35 Range
        [Theory]
        [InlineData("100-")] // Out of range
        [InlineData("1000-1001")] // Out of range
        [InlineData("-0")] // Suffix range must be non-zero
        public async Task SingleNotSatisfiableRange(string range)
        {
            TestServer server = TestServer.Create(app => app.UseFileServer());
            var req = new HttpRequestMessage(HttpMethod.Get, "http://localhost/SubFolder/Ranges.txt");
            req.Headers.TryAddWithoutValidation("Range", "bytes=" + range);
            HttpResponseMessage resp = await server.HttpClient.SendAsync(req);
            Assert.Equal(HttpStatusCode.RequestedRangeNotSatisfiable, resp.StatusCode);
            Assert.Equal("bytes */62", resp.Content.Headers.ContentRange.ToString());
        }

        // 14.35 Range
        [Theory]
        [InlineData("100-")] // Out of range
        [InlineData("1000-1001")] // Out of range
        [InlineData("-0")] // Suffix range must be non-zero
        public async Task HEADSingleNotSatisfiableRange(string range)
        {
            TestServer server = TestServer.Create(app => app.UseFileServer());
            var req = new HttpRequestMessage(HttpMethod.Head, "http://localhost/SubFolder/Ranges.txt");
            req.Headers.TryAddWithoutValidation("Range", "bytes=" + range);
            HttpResponseMessage resp = await server.HttpClient.SendAsync(req);
            Assert.Equal(HttpStatusCode.RequestedRangeNotSatisfiable, resp.StatusCode);
            Assert.Equal("bytes */62", resp.Content.Headers.ContentRange.ToString());
        }

        // 14.35 Range
        [Theory]
        [InlineData("")]
        [InlineData("0")]
        [InlineData("1-0")]
        [InlineData("-")]
        public async Task SingleInvalidRangeIgnored(string range)
        {
            TestServer server = TestServer.Create(app => app.UseFileServer());
            var req = new HttpRequestMessage(HttpMethod.Get, "http://localhost/SubFolder/Ranges.txt");
            req.Headers.TryAddWithoutValidation("Range", "bytes=" + range);
            HttpResponseMessage resp = await server.HttpClient.SendAsync(req);
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
            Assert.Null(resp.Content.Headers.ContentRange);
            Assert.Equal(62, resp.Content.Headers.ContentLength);
            Assert.Equal("0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ", await resp.Content.ReadAsStringAsync());
        }

        // 14.35 Range
        [Theory]
        [InlineData("")]
        [InlineData("0")]
        [InlineData("1-0")]
        [InlineData("-")]
        public async Task HEADSingleInvalidRangeIgnored(string range)
        {
            TestServer server = TestServer.Create(app => app.UseFileServer());
            var req = new HttpRequestMessage(HttpMethod.Head, "http://localhost/SubFolder/Ranges.txt");
            req.Headers.TryAddWithoutValidation("Range", "bytes=" + range);
            HttpResponseMessage resp = await server.HttpClient.SendAsync(req);
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
            Assert.Null(resp.Content.Headers.ContentRange);
            Assert.Equal(62, resp.Content.Headers.ContentLength);
            Assert.Equal(string.Empty, await resp.Content.ReadAsStringAsync());
        }

        // 14.35 Range
        [Theory]
        [InlineData("0-0,2-2", new[] { "0-0", "2-2" }, new[] { "0", "2" })]
        [InlineData("0-0,60-", new[] { "0-0", "60-61" }, new[] { "0", "YZ" })]
        [InlineData("0-0,-2", new[] { "0-0", "60-61" }, new[] { "0", "YZ" })]
        [InlineData("2-2,0-0", new[] { "2-2", "0-0" }, new[] { "2", "0" })] // SHOULD send in the requested order.
        [InlineData("0-0,2-2,4-4,6-6,8-8", new[] { "0-0", "2-2", "4-4", "6-6", "8-8" }, new[] { "0", "2", "4", "6", "8" })]
        [InlineData("0-0,6-6,8-8,2-2,4-4", new[] { "0-0", "6-6", "8-8", "2-2", "4-4" }, new[] { "0", "6", "8", "2", "4" })]
        public async Task MultipleValidRangesShouldServePartialMultipartContent(string range, string[] expectedRanges, string[] expectedData)
        {
            TestServer server = TestServer.Create(app => app.UseFileServer());
            var req = new HttpRequestMessage(HttpMethod.Get, "http://localhost/SubFolder/Ranges.txt");
            req.Headers.Add("Range", "bytes=" + range);
            HttpResponseMessage resp = await server.HttpClient.SendAsync(req);
            Assert.Equal(HttpStatusCode.PartialContent, resp.StatusCode);
            Assert.True(resp.Content.Headers.ContentType.ToString().StartsWith("multipart/byteranges;"));

            var multipart = await resp.Content.ReadAsMultipartAsync();
            IList<HttpContent> contentList = multipart.Contents.ToList();
            Assert.Equal(expectedRanges.Length, contentList.Count);
            for (int i = 0; i < contentList.Count; i++)
            {
                HttpContent content = contentList[i];
                Assert.Equal("text/plain", content.Headers.ContentType.ToString());
                Assert.Equal("bytes " + expectedRanges[i] + "/62", content.Headers.ContentRange.ToString());
                Assert.Equal(expectedData[i], await content.ReadAsStringAsync());
            }
        }

        // 14.35 Range
        [Theory]
        [InlineData("0-0,2-2")]
        [InlineData("0-0,60-")]
        [InlineData("0-0,-2")]
        [InlineData("2-2,0-0")] // SHOULD send in the requested order.
        public async Task HEADMultipleValidRangesShouldServePartialMultipartContent(string range)
        {
            TestServer server = TestServer.Create(app => app.UseFileServer());
            var req = new HttpRequestMessage(HttpMethod.Head, "http://localhost/SubFolder/Ranges.txt");
            req.Headers.Add("Range", "bytes=" + range);
            HttpResponseMessage resp = await server.HttpClient.SendAsync(req);
            Assert.Equal(HttpStatusCode.PartialContent, resp.StatusCode);
            Assert.True(resp.Content.Headers.ContentType.ToString().StartsWith("multipart/byteranges;"));
            Assert.Equal(string.Empty, await resp.Content.ReadAsStringAsync());
        }

        // 14.35 Range consolidation
        [Theory]
        [InlineData("0-0,1-1", new[] { "0-1" }, new[] { "01" })]
        [InlineData("0-0,36-43,1-1", new[] { "0-1", "36-43" }, new[] { "01", "ABCDEFGH" })]
        [InlineData("1-1,36-43,0-0", new[] { "0-1", "36-43" }, new[] { "01", "ABCDEFGH" })]
        [InlineData("0-0,9-9,2-2,7-7,4-4,5-5,6-6,3-3,8-8,1-1", new[] { "0-9" }, new[] { "0123456789" })]
        [InlineData("9-9,2-2,7-7,4-4,5-5,6-6,3-3,8-8,0-0", new[] { "2-9", "0-0" }, new[] { "23456789", "0" })]
        [InlineData("0-0,0-2,0-5,0-9,1-1", new[] { "0-9" }, new[] { "0123456789" })]
        [InlineData("10-,-100", new[] { "0-61" }, new[] { "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ" })]
        public async Task MultipleValidAdjacentRangesShouldConsolidate(string range, string[] expectedRanges, string[] expectedData)
        {
            TestServer server = TestServer.Create(app => app.UseFileServer());
            var req = new HttpRequestMessage(HttpMethod.Get, "http://localhost/SubFolder/Ranges.txt");
            req.Headers.Add("Range", "bytes=" + range);
            HttpResponseMessage resp = await server.HttpClient.SendAsync(req);
            Assert.Equal(HttpStatusCode.PartialContent, resp.StatusCode);

            bool expectMultipart = expectedRanges.Length > 1;
            if (expectMultipart)
            {
                Assert.True(resp.Content.Headers.ContentType.ToString().StartsWith("multipart/byteranges;"));

                var multipart = await resp.Content.ReadAsMultipartAsync();
                IList<HttpContent> contentList = multipart.Contents.ToList();
                Assert.Equal(expectedRanges.Length, contentList.Count);
                for (int i = 0; i < contentList.Count; i++)
                {
                    HttpContent content = contentList[i];
                    Assert.Equal("text/plain", content.Headers.ContentType.ToString());
                    Assert.Equal("bytes " + expectedRanges[i] + "/62", content.Headers.ContentRange.ToString());
                    Assert.Equal(expectedData[i], await content.ReadAsStringAsync());
                }
            }
            else
            {
                Assert.Equal("text/plain", resp.Content.Headers.ContentType.ToString());
                Assert.NotNull(resp.Content.Headers.ContentRange);
                Assert.Equal("bytes " + expectedRanges[0] + "/62", resp.Content.Headers.ContentRange.ToString());
                Assert.Equal(expectedData[0], await resp.Content.ReadAsStringAsync());
            }
        }
    }
}
