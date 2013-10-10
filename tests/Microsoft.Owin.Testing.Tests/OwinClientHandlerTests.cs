// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Owin.Testing.Tests
{
    public class OwinClientHandlerTests
    {
        [Fact]
        public void LeadingQuestionMarkInQueryIsRemoved()
        {
            /* http://katanaproject.codeplex.com/workitem/22
             * 
             * Summary
             * 
             * The owin spec for the "owin.RequestQueryString" key: 
             *    
             *    A string containing the query string component of the HTTP request URI,
             *    without the leading “?” (e.g., "foo=bar&baz=quux"). The value may be an
             *    empty string.
             *    
             *  request.RequestUri.Query does not remove the leading '?'. This causes
             *  problems with hosts that then subsequently join the path and querystring
             *  resulting in a '??' (such as signalr converting the env dict to a ServerRequest) 
             */

            IDictionary<string, object> env = null;
            var handler = new OwinClientHandler(dict =>
            {
                env = dict;
                return TaskHelpers.Completed();
            });
            var httpClient = new HttpClient(handler);
            string query = "a=b";
            httpClient.GetAsync("http://example.com?" + query).Wait();
            Assert.Equal(query, env["owin.RequestQueryString"]);
        }

        [Fact]
        public void ExpectedKeysAreAvailable()
        {
            var handler = new OwinClientHandler(env =>
            {
                IOwinContext context = new OwinContext(env);

                Assert.Equal("1.0", context.Get<string>("owin.Version"));
                Assert.NotNull(context.Get<CancellationToken>("owin.CallCancelled"));
                Assert.Equal("HTTP/1.1", context.Request.Protocol);
                Assert.Equal("GET", context.Request.Method);
                Assert.Equal("https", context.Request.Scheme);
                Assert.Equal(string.Empty, context.Get<string>("owin.RequestPathBase"));
                Assert.Equal("/A/Path/and/file.txt", context.Get<string>("owin.RequestPath"));
                Assert.Equal("and=query", context.Get<string>("owin.RequestQueryString"));
                Assert.NotNull(context.Request.Body);
                Assert.NotNull(context.Get<IDictionary<string, string[]>>("owin.RequestHeaders"));
                Assert.NotNull(context.Get<IDictionary<string, string[]>>("owin.ResponseHeaders"));
                Assert.NotNull(context.Response.Body);
                Assert.Equal(200, context.Get<int>("owin.ResponseStatusCode"));
                Assert.Null(context.Get<string>("owin.ResponseReasonPhrase"));

                Assert.Equal("example.com", context.Request.Headers.Get("Host"));

                return TaskHelpers.Completed();
            });
            var httpClient = new HttpClient(handler);
            httpClient.GetAsync("https://example.com/A/Path/and/file.txt?and=query").Wait();
        }
    }
}
