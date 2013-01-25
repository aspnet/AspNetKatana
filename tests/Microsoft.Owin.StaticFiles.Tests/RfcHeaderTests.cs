using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Owin.Testing;
using Shouldly;
using Xunit;
#pragma warning disable 1998

namespace Microsoft.Owin.StaticFiles.Tests
{
    public class RfcHeaderTests
    {
        [Fact]
        public async Task ServerShouldReturnETag()
        {
            var server = TestServer.Create(app => app.UseFileServer());

            var response = await server.HttpClient.GetAsync("http://localhost/SubFolder/Extra.xml");
            response.Headers.ETag.ShouldNotBe(null);
            response.Headers.ETag.Tag.ShouldNotBe(null);
        }

        [Fact]
        public async Task SameETagShouldBeReturnedAgain()
        {
            var server = TestServer.Create(app => app.UseFileServer());

            var response1 = await server.HttpClient.GetAsync("http://localhost/SubFolder/Extra.xml");
            var response2 = await server.HttpClient.GetAsync("http://localhost/SubFolder/Extra.xml");
            response1.Headers.ETag.ShouldBe(response2.Headers.ETag);
        }

        // 14.24 If-Match
        //If none of the entity tags match, or if "*" is given and no current
        //entity exists, the server MUST NOT perform the requested method, and
        //MUST return a 412 (Precondition Failed) response. This behavior is
        //most useful when the client wants to prevent an updating method, such
        //as PUT, from modifying a resource that has changed since the client
        //last retrieved it.

        [Fact]
        public async Task IfMatchShouldReturn412WhenNotListed()
        {
            var server = TestServer.Create(app => app.UseFileServer());
            var req = new HttpRequestMessage(HttpMethod.Get, "http://localhost/SubFolder/Extra.xml");
            req.Headers.Add("If-Match", "\"fake\"");
            var resp = await server.HttpClient.SendAsync(req);
            resp.StatusCode.ShouldBe(HttpStatusCode.PreconditionFailed);
        }


        [Fact]
        public async Task IfMatchShouldBeServedWhenListed()
        {
            var server = TestServer.Create(app => app.UseFileServer());
            var original = await server.HttpClient.GetAsync("http://localhost/SubFolder/Extra.xml");

            var req = new HttpRequestMessage(HttpMethod.Get, "http://localhost/SubFolder/Extra.xml");
            req.Headers.Add("If-Match", original.Headers.ETag.ToString());
            var resp = await server.HttpClient.SendAsync(req);
            resp.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        // 14.26 If-None-Match
        //If any of the entity tags match the entity tag of the entity that
        //would have been returned in the response to a similar GET request
        //(without the If-None-Match header) on that resource, or if "*" is
        //given and any current entity exists for that resource, then the
        //server MUST NOT perform the requested method, unless required to do
        //so because the resource's modification date fails to match that
        //supplied in an If-Modified-Since header field in the request.
        //Instead, if the request method was GET or HEAD, the server SHOULD
        //respond with a 304 (Not Modified) response, including the cache-
        //related header fields (particularly ETag) of one of the entities that
        //matched. For all other request methods, the server MUST respond with
        //a status of 412 (Precondition Failed).

        [Fact]
        public async Task IfNoneMatchShouldReturn304ForMatchingOnGetAndHeadMethod()
        {
            var server = TestServer.Create(app => app.UseFileServer());
            var resp1 = await server.HttpClient.GetAsync("http://localhost/SubFolder/Extra.xml");

            var req2 = new HttpRequestMessage(HttpMethod.Get, "http://localhost/SubFolder/Extra.xml");
            req2.Headers.Add("If-None-Match", resp1.Headers.ETag.ToString());
            var resp2 = await server.HttpClient.SendAsync(req2);
            resp2.StatusCode.ShouldBe(HttpStatusCode.NotModified);

            var req3 = new HttpRequestMessage(HttpMethod.Head, "http://localhost/SubFolder/Extra.xml");
            req3.Headers.Add("If-None-Match", resp1.Headers.ETag.ToString());
            var resp3 = await server.HttpClient.SendAsync(req3);
            resp3.StatusCode.ShouldBe(HttpStatusCode.NotModified);
        }

        [Fact]
        public async Task IfNoneMatchShouldBeIgnoredForNonTwoHundredAnd304Responses()
        {
            var server = TestServer.Create(app => app.UseFileServer());
            var resp1 = await server.HttpClient.GetAsync("http://localhost/SubFolder/Extra.xml");

            var req2 = new HttpRequestMessage(HttpMethod.Post, "http://localhost/SubFolder/Extra.xml");
            req2.Headers.Add("If-None-Match", resp1.Headers.ETag.ToString());
            var resp2 = await server.HttpClient.SendAsync(req2);
            resp2.StatusCode.ShouldBe(HttpStatusCode.NotFound);

            var req3 = new HttpRequestMessage(HttpMethod.Put, "http://localhost/SubFolder/Extra.xml");
            req3.Headers.Add("If-None-Match", resp1.Headers.ETag.ToString());
            var resp3 = await server.HttpClient.SendAsync(req3);
            resp3.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        }

        // 14.26 If-None-Match
        //If none of the entity tags match, then the server MAY perform the
        //requested method as if the If-None-Match header field did not exist,
        //but MUST also ignore any If-Modified-Since header field(s) in the
        //request. That is, if no entity tags match, then the server MUST NOT
        //return a 304 (Not Modified) response.

        //A server MUST use the strong comparison function (see section 13.3.3)
        //to compare the entity tags in If-Match.

        [Fact]
        public async Task IfMatchReturns()
        {
            // 14.24 
        }

        // 13.3.4
        //An HTTP/1.1 origin server, upon receiving a conditional request that
        //includes both a Last-Modified date (e.g., in an If-Modified-Since or
        //If-Unmodified-Since header field) and one or more entity tags (e.g.,
        //in an If-Match, If-None-Match, or If-Range header field) as cache
        //validators, MUST NOT return a response status of 304 (Not Modified)
        //unless doing so is consistent with all of the conditional header
        //fields in the request.

        [Fact]
        public async Task MatchingBothConditionsReturnsNotModified()
        {
            var server = TestServer.Create(app => app.UseFileServer());
            var resp1 = await server
                .Path("/SubFolder/Extra.xml")
                .SendAsync("GET");

            var resp2 = await server
                .Path("/SubFolder/Extra.xml")
                .Header("If-None-Match", resp1.Headers.ETag.ToString())
                .And(req => req.Headers.IfModifiedSince = resp1.Content.Headers.LastModified)
                .SendAsync("GET");

            resp2.StatusCode.ShouldBe(HttpStatusCode.NotModified);
        }

        [Fact]
        public async Task MissingEitherOrBothConditionsReturnsNormally()
        {
            var server = TestServer.Create(app => app.UseFileServer());
            var resp1 = await server
                .Path("/SubFolder/Extra.xml")
                .SendAsync("GET");

            var resp2 = await server
                .Path("/SubFolder/Extra.xml")
                .Header("If-None-Match", "\"fake\"")
                .And(req => req.Headers.IfModifiedSince = resp1.Content.Headers.LastModified)
                .SendAsync("GET");

            var wrongDate = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromHours(1));

            var resp3 = await server
                .Path("/SubFolder/Extra.xml")
                .Header("If-None-Match", resp1.Headers.ETag.ToString())
                .And(req => req.Headers.IfModifiedSince = wrongDate)
                .SendAsync("GET");

            var resp4 = await server
                .Path("/SubFolder/Extra.xml")
                .Header("If-None-Match", "\"fake\"")
                .And(req => req.Headers.IfModifiedSince = wrongDate)
                .SendAsync("GET");

            resp2.StatusCode.ShouldBe(HttpStatusCode.OK);
            resp3.StatusCode.ShouldBe(HttpStatusCode.OK);
            resp4.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        // 14.25 If-Modified-Since
        //The If-Modified-Since request-header field is used with a method to
        //make it conditional: if the requested variant has not been modified
        //since the time specified in this field, an entity will not be
        //returned from the server; instead, a 304 (not modified) response will
        //be returned without any message-body.

        //a) If the request would normally result in anything other than a
        //   200 (OK) status, or if the passed If-Modified-Since date is
        //   invalid, the response is exactly the same as for a normal GET.
        //   A date which is later than the server's current time is
        //   invalid.
        [Fact]
        public async Task InvalidIfModifiedSinceDateFormatGivesNormalGet()
        {
            var server = TestServer.Create(app => app.UseFileServer());

            var res = await server
                .Path("/SubFolder/Extra.xml")
                .Header("If-Modified-Since", "bad-date")
                .SendAsync("GET");

            res.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        //b) If the variant has been modified since the If-Modified-Since
        //   date, the response is exactly the same as for a normal GET.

        //c) If the variant has not been modified since a valid If-
        //   Modified-Since date, the server SHOULD return a 304 (Not
        //   Modified) response.

        [Fact]
        public async Task IfModifiedSinceDateEqualsLastModifiedShouldReturn304()
        {
            var server = TestServer.Create(app => app.UseFileServer());

            var res1 = await server
                .Path("/SubFolder/Extra.xml")
                .SendAsync("GET");

            var res2 = await server
                .Path("/SubFolder/Extra.xml")
                .And(req => req.Headers.IfModifiedSince = res1.Content.Headers.LastModified)
                .SendAsync("GET");

            res2.StatusCode.ShouldBe(HttpStatusCode.NotModified);
        }

    }
}
