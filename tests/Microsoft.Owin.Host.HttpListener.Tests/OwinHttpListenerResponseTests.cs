// <copyright file="OwinHttpListenerResponseTests.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Owin.Host.HttpListener.Tests
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    /// NOTE: These tests require SetupProject.bat to be run as admin from a VS command prompt once per machine.
    public class OwinHttpListenerResponseTests
    {
        private static readonly string[] HttpServerAddress = new string[] { "http", "localhost", "8080", "/BaseAddress/" };
        private const string HttpClientAddress = "http://localhost:8080/BaseAddress/";

        [Fact]
        public async Task OwinHttpListenerResponse_Empty200Response_Success()
        {
            OwinHttpListener listener = CreateServer(call => TaskHelpers.Completed(), HttpServerAddress);

            using (listener)
            {
                var client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(HttpClientAddress);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("OK", response.ReasonPhrase);
                Assert.Equal(3, response.Headers.Count()); // Date, Chunked, Server
                Assert.True(response.Headers.TransferEncodingChunked.Value);
                Assert.True(response.Headers.Date.HasValue);
                Assert.Equal(1, response.Headers.Server.Count);
                Assert.Equal(string.Empty, await response.Content.ReadAsStringAsync());
            }
        }

        [Fact]
        public async Task ResultParmeters_NullHeaderDictionary_SucceedAnyways()
        {
            OwinHttpListener listener = CreateServer(
                env =>
                {
                    env["owin.ResponseHeaders"] = null;
                    return TaskHelpers.Completed();
                },
                HttpServerAddress);

            using (listener)
            {
                var client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(HttpClientAddress);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }

        [Fact]
        public async Task Headers_CustomHeaders_PassedThrough()
        {
            OwinHttpListener listener = CreateServer(
                env =>
                {
                    var responseHeaders = env.Get<IDictionary<string, string[]>>("owin.ResponseHeaders");
                    responseHeaders.Add("Custom1", new string[] { "value1a", "value1b" });
                    responseHeaders.Add("Custom2", new string[] { "value2a, value2b" });
                    responseHeaders.Add("Custom3", new string[] { "value3a, value3b", "value3c" });
                    return TaskHelpers.Completed();
                },
                HttpServerAddress);

            using (listener)
            {
                var client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(HttpClientAddress);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(6, response.Headers.Count()); // Date, Chunked, Server

                Assert.Equal(2, response.Headers.GetValues("Custom1").Count());
                Assert.Equal("value1a", response.Headers.GetValues("Custom1").First());
                Assert.Equal("value1b", response.Headers.GetValues("Custom1").Skip(1).First());
                Assert.Equal(1, response.Headers.GetValues("Custom2").Count());
                Assert.Equal("value2a, value2b", response.Headers.GetValues("Custom2").First());
                Assert.Equal(2, response.Headers.GetValues("Custom3").Count());
                Assert.Equal("value3a, value3b", response.Headers.GetValues("Custom3").First());
                Assert.Equal("value3c", response.Headers.GetValues("Custom3").Skip(1).First());
            }
        }

        [Fact]
        public async Task Headers_ReservedHeaders_PassedThrough()
        {
            OwinHttpListener listener = CreateServer(
                env =>
                {
                    var responseHeaders = env.Get<IDictionary<string, string[]>>("owin.ResponseHeaders");
                    env.Add("owin.ResponseProtocol", "HTTP/1.0");
                    responseHeaders.Add("KEEP-alive", new string[] { "TRUE" });
                    responseHeaders.Add("content-length", new string[] { "0" });
                    responseHeaders.Add("www-Authenticate", new string[] { "Basic", "NTLM" });
                    return TaskHelpers.Completed();
                },
                HttpServerAddress);

            using (listener)
            {
                var client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(HttpClientAddress);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(3, response.Headers.Count()); // Date, Server
                Assert.Equal(0, response.Content.Headers.ContentLength);
                Assert.Equal(2, response.Headers.WwwAuthenticate.Count());

                // The client does not expose KeepAlive
            }
        }

        [Fact]
        public async Task Headers_OtherReservedHeaders_PassedThrough()
        {
            OwinHttpListener listener = CreateServer(
                env =>
                {
                    var responseHeaders = env.Get<IDictionary<string, string[]>>("owin.ResponseHeaders");
                    responseHeaders.Add("Transfer-Encoding", new string[] { "ChUnKed" });
                    responseHeaders.Add("CONNECTION", new string[] { "ClOsE" });
                    return TaskHelpers.Completed();
                },
                HttpServerAddress);

            using (listener)
            {
                var client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(HttpClientAddress);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(4, response.Headers.Count()); // Date, Server
                Assert.Equal("chunked", response.Headers.TransferEncoding.ToString()); // Normalized by server
                Assert.True(response.Headers.TransferEncodingChunked.Value);
                Assert.Equal("close", response.Headers.Connection.First()); // Normalized by server
                Assert.True(response.Headers.ConnectionClose.Value);
            }
        }

        [Fact]
        public async Task Headers_BadContentLength_ConnectionClosed()
        {
            OwinHttpListener listener = CreateServer(
                env =>
                {
                    var responseHeaders = env.Get<IDictionary<string, string[]>>("owin.ResponseHeaders");
                    responseHeaders.Add("content-length", new string[] { "-10" });
                    return TaskHelpers.Completed();
                },
                HttpServerAddress);

            using (listener)
            {
                var client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(HttpClientAddress);
                Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
                Assert.Equal(0, response.Content.Headers.ContentLength.Value);
            }
        }

        [Fact]
        public async Task Properties_CustomReasonPhrase_PassedThrough()
        {
            OwinHttpListener listener = CreateServer(
                env =>
                {
                    env.Add("owin.ResponseReasonPhrase", "Awesome");
                    return TaskHelpers.Completed();
                },
                HttpServerAddress);

            using (listener)
            {
                var client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(HttpClientAddress);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("Awesome", response.ReasonPhrase);
            }
        }

        [Fact]
        public async Task Properties_BadReasonPhrase_Throws()
        {
            OwinHttpListener listener = CreateServer(
                env =>
                {
                    env.Add("owin.ResponseReasonPhrase", int.MaxValue);
                    return TaskHelpers.Completed();
                },
                HttpServerAddress);

            using (listener)
            {
                var client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(HttpClientAddress);
                Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            }
        }

        // Note that Http.Sys does not allow HTTP/1.0 responses.
        [Fact]
        public async Task Properties_HTTP10Protocol_NotPassedThrough()
        {
            OwinHttpListener listener = CreateServer(
                env =>
                {
                    env.Add("owin.ResponseProtocol", "http/1.0");
                    return TaskHelpers.Completed();
                },
                HttpServerAddress);

            using (listener)
            {
                var client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(HttpClientAddress);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(new Version(1, 1), response.Version);
            }
        }

        [Fact]
        public async Task Properties_UnknownProtocol_ConnectionClosed()
        {
            OwinHttpListener listener = CreateServer(
                env =>
                {
                    env.Add("owin.ResponseProtocol", "http/2.0");
                    return TaskHelpers.Completed();
                },
                HttpServerAddress);

            using (listener)
            {
                var client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(HttpClientAddress);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(new Version(1, 1), response.Version);
            }
        }

        [Fact]
        public async Task Body_SmallChunked_Success()
        {
            OwinHttpListener listener = CreateServer(
                env =>
                {
                    var responseStream = env.Get<Stream>("owin.ResponseBody");
                    responseStream.Write(new byte[10], 0, 10);
                    return TaskHelpers.Completed();
                },
                HttpServerAddress);

            using (listener)
            {
                var client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(HttpClientAddress);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(10, (await response.Content.ReadAsByteArrayAsync()).Length);
            }
        }

        [Fact]
        public async Task Body_LargeChunked_Success()
        {
            OwinHttpListener listener = CreateServer(
                async env =>
                {
                    var responseStream = env.Get<Stream>("owin.ResponseBody");
                    for (int i = 0; i < 100; i++)
                    {
                        await responseStream.WriteAsync(new byte[1000], 0, 1000);
                    }
                },
                HttpServerAddress);

            using (listener)
            {
                var client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(HttpClientAddress);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(100 * 1000, (await response.Content.ReadAsByteArrayAsync()).Length);
            }
        }

        [Fact]
        public void Body_SmallerThanContentLength_ConnectionClosed()
        {
            OwinHttpListener listener = CreateServer(
                env =>
                {
                    var responseHeaders = env.Get<IDictionary<string, string[]>>("owin.ResponseHeaders");
                    responseHeaders.Add("Content-Length", new string[] { "100" });
                    var responseStream = env.Get<Stream>("owin.ResponseBody");
                    responseStream.Write(new byte[95], 0, 95);
                    return TaskHelpers.Completed();
                },
                HttpServerAddress);

            using (listener)
            {
                var client = new HttpClient();
                Assert.Throws<AggregateException>(() => client.GetAsync(HttpClientAddress).Result);
            }
        }

        [Fact]
        public void Body_LargerThanContentLength_ConnectionClosed()
        {
            OwinHttpListener listener = CreateServer(
                env =>
                {
                    var responseHeaders = env.Get<IDictionary<string, string[]>>("owin.ResponseHeaders");
                    responseHeaders.Add("Content-Length", new string[] { "100" });
                    var responseStream = env.Get<Stream>("owin.ResponseBody");
                    responseStream.Write(new byte[105], 0, 105);
                    return TaskHelpers.Completed();
                },
                HttpServerAddress);

            using (listener)
            {
                var client = new HttpClient();
                Assert.Throws<AggregateException>(() => client.GetAsync(HttpClientAddress).Result);
            }
        }

        [Fact]
        public async Task EndToEnd_AppReturns100Continue_ConnectionClosed()
        {
            OwinHttpListener listener = CreateServer(
                env =>
                {
                    env["owin.ResponseStatusCode"] = 100;
                    return TaskHelpers.Completed();
                },
                HttpServerAddress);

            using (listener)
            {
                var client = new HttpClient();
                string dataString = "Hello World";
                HttpResponseMessage response = await client.PostAsync(HttpClientAddress, new StringContent(dataString));
                Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            }
        }

        [Fact]
        public async Task OwinHttpListenerResponse_Empty101Response_Success()
        {
            OwinHttpListener listener = CreateServer(
                env =>
                {
                    env["owin.ResponseStatusCode"] = 101;
                    return TaskHelpers.Completed();
                },
                HttpServerAddress);

            using (listener)
            {
                var client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(HttpClientAddress);
                Assert.Equal(HttpStatusCode.SwitchingProtocols, response.StatusCode);
                Assert.Equal("Switching Protocols", response.ReasonPhrase);
                Assert.Equal(2, response.Headers.Count()); // Date, Server
                Assert.True(response.Headers.Date.HasValue);
                Assert.Equal(1, response.Headers.Server.Count);
                Assert.Equal(string.Empty, await response.Content.ReadAsStringAsync());
            }
        }

        [Fact]
        public async Task OwinHttpListenerResponse_101ResponseWithBody_BodyIgnoredByClient()
        {
            OwinHttpListener listener = CreateServer(
                env =>
                {
                    env["owin.ResponseStatusCode"] = 101;
                    var responseStream = env.Get<Stream>("owin.ResponseBody");

                    var responseHeaders = env.Get<IDictionary<string, string[]>>("owin.ResponseHeaders");
                    responseHeaders["content-length"] = new string[] { "10" };

                    responseStream.Write(new byte[10], 0, 10);
                    return TaskHelpers.Completed();
                },
                HttpServerAddress);

            using (listener)
            {
                var client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(HttpClientAddress);
                Assert.Equal(HttpStatusCode.SwitchingProtocols, response.StatusCode);
                Assert.Equal("Switching Protocols", response.ReasonPhrase);
                Assert.Equal(2, response.Headers.Count()); // Date, Server
                Assert.True(response.Headers.Date.HasValue);
                Assert.Equal(1, response.Headers.Server.Count);
                Assert.Equal(0, (await response.Content.ReadAsByteArrayAsync()).Length);
            }
        }

        [Fact]
        public async Task OwinHttpListenerResponse_OnFirstWrite_OnSendingHeaders()
        {
            OwinHttpListener listener = CreateServer(
                env =>
                {
                    env["owin.ResponseStatusCode"] = 200;
                    env["owin.ResponseReasonPhrase"] = "Custom";
                    var responseStream = env.Get<Stream>("owin.ResponseBody");

                    var responseHeaders = env.Get<IDictionary<string, string[]>>("owin.ResponseHeaders");

                    env.Get<Action<Action<object>, object>>("server.OnSendingHeaders")(state => responseHeaders["custom-header"] = new string[] { "customvalue" }, null);

                    responseHeaders["content-length"] = new string[] { "10" };

                    responseStream.Write(new byte[10], 0, 10);

                    return TaskHelpers.Completed();
                },
                HttpServerAddress);

            using (listener)
            {
                var client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(HttpClientAddress);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("Custom", response.ReasonPhrase);
                Assert.Equal(3, response.Headers.Count()); // Date, Server
                Assert.True(response.Headers.Date.HasValue);
                Assert.Equal(1, response.Headers.Server.Count);
                Assert.Equal("customvalue", response.Headers.GetValues("custom-header").First());
                Assert.Equal(10, (await response.Content.ReadAsByteArrayAsync()).Length);
            }
        }

        [Fact]
        public async Task OwinHttpListenerResponse_NoWrite_OnSendingHeaders()
        {
            OwinHttpListener listener = CreateServer(
                env =>
                {
                    env["owin.ResponseStatusCode"] = 200;
                    env["owin.ResponseReasonPhrase"] = "Custom";
                    var responseStream = env.Get<Stream>("owin.ResponseBody");

                    var responseHeaders = env.Get<IDictionary<string, string[]>>("owin.ResponseHeaders");

                    env.Get<Action<Action<object>, object>>("server.OnSendingHeaders")(state =>
                    {
                        env["owin.ResponseStatusCode"] = 201;
                        env["owin.ResponseReasonPhrase"] = "Custom1";
                        responseHeaders["custom-header"] = new string[] { "customvalue" };
                    }, null);

                    responseHeaders["content-length"] = new string[] { "0" };
                    return TaskHelpers.Completed();
                },
                HttpServerAddress);

            using (listener)
            {
                var client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(HttpClientAddress);
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.Equal("Custom1", response.ReasonPhrase);
                Assert.Equal(3, response.Headers.Count()); // Date, Server
                Assert.True(response.Headers.Date.HasValue);
                Assert.Equal(1, response.Headers.Server.Count);
                Assert.Equal("customvalue", response.Headers.GetValues("custom-header").First());
                Assert.Equal(0, (await response.Content.ReadAsByteArrayAsync()).Length);
            }
        }

        private OwinHttpListener CreateServer(AppFunc app, string[] addressParts)
        {
            var wrapper = new OwinHttpListener();
            wrapper.Start(wrapper.Listener, app, CreateAddress(addressParts), null);
            return wrapper;
        }

        private static IList<IDictionary<string, object>> CreateAddress(string[] addressParts)
        {
            var address = new Dictionary<string, object>();
            address["scheme"] = addressParts[0];
            address["host"] = addressParts[1];
            address["port"] = addressParts[2];
            address["path"] = addressParts[3];

            IList<IDictionary<string, object>> list = new List<IDictionary<string, object>>();
            list.Add(address);
            return list;
        }
    }
}
