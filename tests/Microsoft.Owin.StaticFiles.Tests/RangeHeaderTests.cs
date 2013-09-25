// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
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

            req = new HttpRequestMessage(HttpMethod.Get, "http://localhost/SubFolder/Ranges.txt");
            req.Headers.Add("If-Range", original.Content.Headers.LastModified.Value.ToString("r"));
            resp = await server.HttpClient.SendAsync(req);
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
            Assert.Null(resp.Content.Headers.ContentRange);
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
        }
    }
}
