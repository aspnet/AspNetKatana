// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Owin.Diagnostics;
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

        private readonly KeyValuePair<string, string>[] _specialHeaders = new KeyValuePair<string, string>[]
        {
            new KeyValuePair<string, string>("Cache-Control", "no-cache=\"field\""),
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
                Assert.False(context.Response.Headers.ContainsKey("custom"));
                context.Response.Headers["custom"] = "custom value";
                Assert.True(context.Response.Headers.ContainsKey("custom"));
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

        public void SetSpecialHeadersApp(IAppBuilder app)
        {
            app.UseErrorPage(new ErrorPageOptions() { ShowExceptionDetails = true });
            app.Run(context =>
            {
                IHeaderDictionary responseHeaders = context.Response.Headers;
                foreach (var header in _specialHeaders)
                {
                    responseHeaders[header.Key] = header.Value;
                    Assert.True(responseHeaders.ContainsKey(header.Key), header.Key);
                    Assert.Equal(header.Value, responseHeaders[header.Key]);
                }

                Assert.Equal(_specialHeaders.Length, responseHeaders.Count);
                Assert.Equal(_specialHeaders.Length, responseHeaders.Count());

                // All show up in enumeration?
                foreach (var specialPair in _specialHeaders)
                {
                    Assert.True(responseHeaders.Select(pair => pair.Key)
                        .Contains(specialPair.Key), specialPair.Key);
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
        public Task SetSpecialHeaders(string serverName)
        {
            int port = RunWebServer(
                serverName,
                SetSpecialHeadersApp);

            var client = new HttpClient();
            return client.GetAsync("http://localhost:" + port + "/special")
                .Then(response =>
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    Assert.Equal("Hello World", result);
                    Assert.Equal((HttpStatusCode)ExpectedStatusCode, response.StatusCode);

                    foreach (var header in _specialHeaders)
                    {
                        IEnumerable<string> values;
                        bool exists = response.Headers.TryGetValues(header.Key, out values)
                            || response.Content.Headers.TryGetValues(header.Key, out values);
                        Assert.True(exists);
                        Assert.Equal(header.Value, values.First());
                    }
                });
        }

        public void SetCacheControlApp(IAppBuilder app)
        {
            app.UseErrorPage(new ErrorPageOptions() { ShowExceptionDetails = true });
            app.Run(context =>
            {
                StreamReader reader = new StreamReader(context.Request.Body);
                string cacheHeader = reader.ReadToEnd();
                reader.Dispose();

                if (!cacheHeader.Equals("DontSet"))
                {
                    bool append = "Append".Equals(context.Request.QueryString.Value);

                    if (append)
                    {
                        foreach (var value in cacheHeader.Split('\0'))
                        {
                            context.Response.Headers.AppendValues("Cache-Control", value);
                        }
                    }
                    else
                    {
                        context.Response.Headers.SetValues("Cache-Control", cacheHeader.Split('\0'));
                    }

                    string roundTrip = context.Response.Headers.Get("Cache-Control");
                    Assert.Equal(string.Join(",", cacheHeader.Split('\0')), roundTrip);
                }

                // Some header issues are only visible after calling write and flush.
                context.Response.Write("Hello World");
                context.Response.Body.Flush();
                return TaskHelpers.Completed();
            });
        }

        [Theory]
        [InlineData("public", "public")]
        [InlineData("private", "private")]
        [InlineData("public\0private=\"field\"", "public, private=\"field\"")]
        [InlineData("no-cache", "no-cache")]
        [InlineData("public, no-cache=\"field\"", "public, no-cache=\"field\"")]
        [InlineData("no-store", "no-store")]
        [InlineData("no-transform", "no-transform")]
        [InlineData("must-revalidate", "must-revalidate")]
        [InlineData("proxy-revalidate", "proxy-revalidate")]
        [InlineData("max-age=1000", "max-age=1000")]
        [InlineData("s-maxage=1001", "s-maxage=1001")]
        [InlineData("public, max-age=1002", "public, max-age=1002")]
        [InlineData("", "")]
        [InlineData("DontSet", "private")]
        [InlineData("public\0private\0no-cache\0no-store\0no-transform\0must-revalidate\0proxy-revalidate\0max-age=1000\0s-maxage=1001",
            "no-store, no-transform, public, must-revalidate, proxy-revalidate, no-cache, max-age=1000, s-maxage=1001, private")]
        public void SetCacheControl(string value, string expected)
        {
            int port = RunWebServer("Microsoft.Owin.Host.SystemWeb", SetCacheControlApp);

            var client = new HttpClient();
            var response = client.PostAsync("http://localhost:" + port + "/cache-control", new StringContent(value)).Result;
            Assert.Equal("Hello World", response.Content.ReadAsStringAsync().Result);
            Assert.Equal(expected, Convert.ToString(response.Headers.CacheControl));

            response = client.PostAsync("http://localhost:" + port + "/cache-control?Append", new StringContent(value)).Result;
            Assert.Equal("Hello World", response.Content.ReadAsStringAsync().Result);
            Assert.Equal(expected, Convert.ToString(response.Headers.CacheControl));
        }
    }
}
