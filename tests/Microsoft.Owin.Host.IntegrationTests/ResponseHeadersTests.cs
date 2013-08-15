// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Owin;
using Xunit;
using Xunit.Extensions;

#if NET40
namespace Microsoft.Owin.Host40.IntegrationTests
#else

namespace Microsoft.Owin.Host45.IntegrationTests
#endif
{
    public class ResponseHeadersTests : TestBase
    {
        private const int ExpectedStatusCode = 201;

        // Cache-control and Expires are mutually exclusive.
        private readonly KeyValuePair<string, string>[] _specialHeadersA = new KeyValuePair<string, string>[]
        {
            new KeyValuePair<string, string>("Cache-Control", "no-cache"),
            new KeyValuePair<string, string>("Content-Encoding", "special"),
            new KeyValuePair<string, string>("Content-Length", "11"),
            new KeyValuePair<string, string>("Content-Type", "text/plain"),
            new KeyValuePair<string, string>("Location", "/"),
        };

        // Cache-control and Expires are mutually exclusive.
        private readonly KeyValuePair<string, string>[] _specialHeadersB = new KeyValuePair<string, string>[]
        {
            new KeyValuePair<string, string>("Content-Encoding", "special"),
            new KeyValuePair<string, string>("Content-Length", "11"),
            new KeyValuePair<string, string>("Content-Type", "text/plain"),
            new KeyValuePair<string, string>("Expires", "Mon, 01 Jul 2023 19:49:38 GMT"),
            new KeyValuePair<string, string>("Location", "/"),
        };

        public void SetCustomResponseHeader(IAppBuilder app)
        {
            app.Run(context =>
            {
                context.Response.Headers["custom"] = "custom value";
                context.Response.StatusCode = ExpectedStatusCode;
                return TaskHelpers.Completed();
            });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task SetCustomHeaders_Success(string serverName)
        {
            int port = RunWebServer(
                serverName,
                SetCustomResponseHeader);

            var client = new HttpClient();
            return client.GetAsync("http://localhost:" + port + "/custom")
                         .Then(response =>
                         {
                             Assert.Equal((HttpStatusCode)ExpectedStatusCode, response.StatusCode);
                             Assert.Equal("custom value", response.Headers.GetValues("custom").First());
                         });
        }

        public void SetDuplicateResponseHeader(IAppBuilder app)
        {
            app.Run(context =>
            {
                context.Response.Headers.Add("DummyHeader", new string[] { "DummyHeaderValue" });
                context.Response.Headers.Add("DummyHeader", new string[] { "DummyHeaderValue" });
                context.Response.StatusCode = ExpectedStatusCode;
                return TaskHelpers.Completed();
            });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task SetDuplicateHeader(string serverName)
        {
            int port = RunWebServer(
                serverName,
                SetDuplicateResponseHeader);

            var client = new HttpClient();
            return client.GetAsync("http://localhost:" + port + "/duplicate")
                         .Then(response => { Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode); });
        }

        public void SetSpecialResponseHeadersA(IAppBuilder app)
        {
            app.Run(context =>
            {
                IHeaderDictionary responseHeaders = context.Response.Headers;
                foreach (var header in _specialHeadersA)
                {
                    responseHeaders[header.Key] = header.Value;
                }

                context.Response.StatusCode = ExpectedStatusCode;

                // Some header issues are only visible after calling write and flush.
                context.Response.Write("Hello World");
                context.Response.Body.Flush();
                return TaskHelpers.Completed();
            });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task SetSpecialHeadersA(string serverName)
        {
            int port = RunWebServer(
                serverName,
                SetSpecialResponseHeadersA);

            var client = new HttpClient();
            return client.GetAsync("http://localhost:" + port + "/special")
                         .Then(response =>
                         {
                             Assert.Equal((HttpStatusCode)ExpectedStatusCode, response.StatusCode);

                             foreach (var header in _specialHeadersA)
                             {
                                 IEnumerable<string> values;
                                 bool exists = response.Headers.TryGetValues(header.Key, out values)
                                     || response.Content.Headers.TryGetValues(header.Key, out values);
                                 Assert.True(exists);
                                 Assert.Equal(header.Value, values.First());
                             }
                         });
        }

        public void SetSpecialResponseHeadersB(IAppBuilder app)
        {
            app.Run(context =>
            {
                IHeaderDictionary responseHeaders = context.Response.Headers;
                foreach (var header in _specialHeadersB)
                {
                    responseHeaders[header.Key] = header.Value;
                }

                context.Response.StatusCode = ExpectedStatusCode;

                // Some header issues are only visible after calling write and flush.
                context.Response.Write("Hello World");
                context.Response.Body.Flush();
                return TaskHelpers.Completed();
            });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task SetSpecialHeadersB(string serverName)
        {
            int port = RunWebServer(
                serverName,
                SetSpecialResponseHeadersB);

            var client = new HttpClient();
            return client.GetAsync("http://localhost:" + port + "/special")
                         .Then(response =>
                         {
                             Assert.Equal((HttpStatusCode)ExpectedStatusCode, response.StatusCode);

                             foreach (var header in _specialHeadersB)
                             {
                                 IEnumerable<string> values;
                                 bool exists = response.Headers.TryGetValues(header.Key, out values)
                                     || response.Content.Headers.TryGetValues(header.Key, out values);
                                 Assert.True(exists);
                                 Assert.Equal(header.Value, values.First());
                             }
                         });
        }
    }
}
