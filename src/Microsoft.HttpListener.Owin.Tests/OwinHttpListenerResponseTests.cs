// <copyright file="OwinHttpListenerResponseTests.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
// </copyright>
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.HttpListener.Owin.Tests
{
    /// NOTE: These tests require SetupProject.bat to be run as admin from a VS command prompt once per machine.
    [TestClass]
    public class OwinHttpListenerResponseTests
    {
        private static readonly string[] HttpServerAddress = new string[] { "http://*:8080/BaseAddress/" };
        private const string HttpClientAddress = "http://localhost:8080/BaseAddress/";
        private static readonly string[] HttpsServerAddress = new string[] { "https://*:9090/BaseAddress/" };
        private const string HttpsClientAddress = "https://localhost:9090/BaseAddress/";

        [TestMethod]
        public async Task OwinHttpListenerResponse_Empty200Response_Success()
        {
            OwinHttpListener listener = new OwinHttpListener(call => TaskHelpers.Completed(), HttpServerAddress, null);

            using (listener)
            {
                listener.Start();
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(HttpClientAddress);
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                Assert.AreEqual("OK", response.ReasonPhrase);
                Assert.AreEqual(3, response.Headers.Count()); // Date, Chunked, Server
                Assert.IsTrue(response.Headers.TransferEncodingChunked.Value);
                Assert.IsTrue(response.Headers.Date.HasValue);
                Assert.AreEqual(1, response.Headers.Server.Count);
                Assert.AreEqual(string.Empty, await response.Content.ReadAsStringAsync());
            }
        }

        [TestMethod]
        public async Task ResultParmeters_NullHeaderDictionary_SucceedAnyways()
        {
            OwinHttpListener listener = new OwinHttpListener(
                env =>
                {
                    env["owin.ResponseHeaders"] = null;
                    return TaskHelpers.Completed();
                },
                HttpServerAddress, null);

            using (listener)
            {
                listener.Start();
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(HttpClientAddress);
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            }
        }

        [TestMethod]
        public async Task Headers_CustomHeaders_PassedThrough()
        {
            OwinHttpListener listener = new OwinHttpListener(
                env =>
                {
                    var responseHeaders = env.Get<IDictionary<string, string[]>>("owin.ResponseHeaders");
                    responseHeaders.Add("Custom1", new string[] { "value1a", "value1b" });
                    responseHeaders.Add("Custom2", new string[] { "value2a, value2b" });
                    responseHeaders.Add("Custom3", new string[] { "value3a, value3b", "value3c" });
                    return TaskHelpers.Completed();
                },
                HttpServerAddress, null);

            using (listener)
            {
                listener.Start();
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(HttpClientAddress);
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                Assert.AreEqual(6, response.Headers.Count()); // Date, Chunked, Server

                Assert.AreEqual(2, response.Headers.GetValues("Custom1").Count());
                Assert.AreEqual("value1a", response.Headers.GetValues("Custom1").First());
                Assert.AreEqual("value1b", response.Headers.GetValues("Custom1").Skip(1).First());
                Assert.AreEqual(1, response.Headers.GetValues("Custom2").Count());
                Assert.AreEqual("value2a, value2b", response.Headers.GetValues("Custom2").First());
                Assert.AreEqual(2, response.Headers.GetValues("Custom3").Count());
                Assert.AreEqual("value3a, value3b", response.Headers.GetValues("Custom3").First());
                Assert.AreEqual("value3c", response.Headers.GetValues("Custom3").Skip(1).First());
            }
        }

        [TestMethod]
        public async Task Headers_ReservedHeaders_PassedThrough()
        {
            OwinHttpListener listener = new OwinHttpListener(
                env =>
                {
                    var responseHeaders = env.Get<IDictionary<string, string[]>>("owin.ResponseHeaders");
                    env.Add("owin.ResponseProtocol", "HTTP/1.0");
                    responseHeaders.Add("KEEP-alive", new string[] { "TRUE" });
                    responseHeaders.Add("content-length", new string[] { "0" });
                    responseHeaders.Add("www-Authenticate", new string[] { "Basic", "NTLM" });
                    return TaskHelpers.Completed();
                },
                HttpServerAddress, null);

            using (listener)
            {
                listener.Start();
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(HttpClientAddress);
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                Assert.AreEqual(3, response.Headers.Count()); // Date, Server
                Assert.AreEqual(0, response.Content.Headers.ContentLength);
                Assert.AreEqual(2, response.Headers.WwwAuthenticate.Count());

                // The client does not expose KeepAlive
            }
        }

        [TestMethod]
        public async Task Headers_OtherReservedHeaders_PassedThrough()
        {
            OwinHttpListener listener = new OwinHttpListener(
                env =>
                {
                    var responseHeaders = env.Get<IDictionary<string, string[]>>("owin.ResponseHeaders");
                    responseHeaders.Add("Transfer-Encoding", new string[] { "ChUnKed" });
                    responseHeaders.Add("CONNECTION", new string[] { "ClOsE" });
                    return TaskHelpers.Completed();
                },
                HttpServerAddress, null);

            using (listener)
            {
                listener.Start();
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(HttpClientAddress);
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                Assert.AreEqual(4, response.Headers.Count()); // Date, Server
                Assert.AreEqual("chunked", response.Headers.TransferEncoding.ToString()); // Normalized by server
                Assert.IsTrue(response.Headers.TransferEncodingChunked.Value);
                Assert.AreEqual("close", response.Headers.Connection.First()); // Normalized by server
                Assert.IsTrue(response.Headers.ConnectionClose.Value);
            }
        }

        [TestMethod]
        public async Task Headers_BadContentLength_ConnectionClosed()
        {
            OwinHttpListener listener = new OwinHttpListener(
                env =>
                {
                    var responseHeaders = env.Get<IDictionary<string, string[]>>("owin.ResponseHeaders");
                    responseHeaders.Add("content-length", new string[] { "-10" });
                    return TaskHelpers.Completed();
                },
                HttpServerAddress, null);

            using (listener)
            {
                listener.Start();
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(HttpClientAddress);
                Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
                Assert.AreEqual(0, response.Content.Headers.ContentLength.Value);
            }
        }

        [TestMethod]
        public async Task Properties_CustomReasonPhrase_PassedThrough()
        {
            OwinHttpListener listener = new OwinHttpListener(
                env =>
                {
                    env.Add("owin.ResponseReasonPhrase", "Awesome");
                    return TaskHelpers.Completed();
                },
                HttpServerAddress, null);

            using (listener)
            {
                listener.Start();
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(HttpClientAddress);
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                Assert.AreEqual("Awesome", response.ReasonPhrase);
            }
        }

        [TestMethod]
        public async Task Properties_BadReasonPhrase_ConnectionClosed()
        {
            OwinHttpListener listener = new OwinHttpListener(
                env =>
                {
                    env.Add("owin.ResponseReasonPhrase", int.MaxValue);
                    // TODO: On First Write isn't being triggerd, so the reason phrase isn't being set.
                    return TaskHelpers.Completed();
                },
                HttpServerAddress, null);

            using (listener)
            {
                listener.Start();
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(HttpClientAddress);
                Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
            }
        }

        // Note that Http.Sys does not allow HTTP/1.0 responses.
        [TestMethod]
        public async Task Properties_HTTP10Protocol_NotPassedThrough()
        {
            OwinHttpListener listener = new OwinHttpListener(
                env =>
                {
                    env.Add("owin.ResponseProtocol", "http/1.0");
                    return TaskHelpers.Completed();
                },
                HttpServerAddress, null);

            using (listener)
            {
                listener.Start();
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(HttpClientAddress);
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                Assert.AreEqual(new Version(1, 1), response.Version);
            }
        }

        [TestMethod]
        public async Task Properties_UnknownProtocol_ConnectionClosed()
        {
            OwinHttpListener listener = new OwinHttpListener(
                env =>
                {
                    env.Add("owin.ResponseProtocol", "http/2.0");
                    return TaskHelpers.Completed();
                },
                HttpServerAddress, null);

            using (listener)
            {
                listener.Start();
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(HttpClientAddress);
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                Assert.AreEqual(new Version(1, 1), response.Version);
            }
        }

        [TestMethod]
        public async Task Body_SmallChunked_Success()
        {
            OwinHttpListener listener = new OwinHttpListener(
                env =>
                {
                    Stream responseStream = env.Get<Stream>("owin.ResponseBody");
                    responseStream.Write(new byte[10], 0, 10);
                    return TaskHelpers.Completed();
                },
                HttpServerAddress, null);

            using (listener)
            {
                listener.Start();
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(HttpClientAddress);
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                Assert.AreEqual(10, (await response.Content.ReadAsByteArrayAsync()).Length);
            }
        }

        [TestMethod]
        public async Task Body_LargeChunked_Success()
        {
            OwinHttpListener listener = new OwinHttpListener(
                async env =>
                {
                    Stream responseStream = env.Get<Stream>("owin.ResponseBody");
                    for (int i = 0; i < 100; i++)
                    {
                        await responseStream.WriteAsync(new byte[1000], 0, 1000);
                    }
                },
                HttpServerAddress, null);

            using (listener)
            {
                listener.Start();
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(HttpClientAddress);
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                Assert.AreEqual(100 * 1000, (await response.Content.ReadAsByteArrayAsync()).Length);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpRequestException))]
        public async Task Body_SmallerThanContentLength_ConnectionClosed()
        {
            OwinHttpListener listener = new OwinHttpListener(
                env =>
                {
                    var responseHeaders = env.Get<IDictionary<string, string[]>>("owin.ResponseHeaders");
                    responseHeaders.Add("Content-Length", new string[] { "100" });
                    Stream responseStream = env.Get<Stream>("owin.ResponseBody");
                    responseStream.Write(new byte[95], 0, 95);
                    return TaskHelpers.Completed();
                },
                HttpServerAddress, null);

            using (listener)
            {
                listener.Start();
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(HttpClientAddress);
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpRequestException))]
        public async Task Body_LargerThanContentLength_ConnectionClosed()
        {
            OwinHttpListener listener = new OwinHttpListener(
                env =>
                {
                    var responseHeaders = env.Get<IDictionary<string, string[]>>("owin.ResponseHeaders");
                    responseHeaders.Add("Content-Length", new string[] { "100" });
                    Stream responseStream = env.Get<Stream>("owin.ResponseBody");
                    responseStream.Write(new byte[105], 0, 105);
                    return TaskHelpers.Completed();
                },
                HttpServerAddress, null);

            using (listener)
            {
                listener.Start();
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(HttpClientAddress);
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                Assert.AreEqual(105, (await response.Content.ReadAsByteArrayAsync()).Length);
            }
        }

        [TestMethod]
        public async Task EndToEnd_AppReturns100Continue_ConnectionClosed()
        {
            OwinHttpListener listener = new OwinHttpListener(
                env =>
                {
                    env["owin.ResponseStatusCode"] = 100;
                    return TaskHelpers.Completed();
                },
                HttpServerAddress, null);

            using (listener)
            {
                listener.Start();
                HttpClient client = new HttpClient();
                string dataString = "Hello World";
                HttpResponseMessage response = await client.PostAsync(HttpClientAddress, new StringContent(dataString));
                Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
            }
        }

        [TestMethod]
        public async Task OwinHttpListenerResponse_Empty101Response_Success()
        {
            OwinHttpListener listener = new OwinHttpListener(
                env =>
                {
                    env["owin.ResponseStatusCode"] = 101;
                    return TaskHelpers.Completed();
                },
                HttpServerAddress, null);

            using (listener)
            {
                listener.Start();
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(HttpClientAddress);
                Assert.AreEqual(HttpStatusCode.SwitchingProtocols, response.StatusCode);
                Assert.AreEqual("Switching Protocols", response.ReasonPhrase);
                Assert.AreEqual(2, response.Headers.Count()); // Date, Server
                Assert.IsTrue(response.Headers.Date.HasValue);
                Assert.AreEqual(1, response.Headers.Server.Count);
                Assert.AreEqual(string.Empty, await response.Content.ReadAsStringAsync());
            }
        }

        [TestMethod]
        public async Task OwinHttpListenerResponse_101ResponseWithBody_BodyIgnoredByClient()
        {
            OwinHttpListener listener = new OwinHttpListener(
                env =>
                {
                    env["owin.ResponseStatusCode"] = 101;
                    Stream responseStream = env.Get<Stream>("owin.ResponseBody");

                    var responseHeaders = env.Get<IDictionary<string, string[]>>("owin.ResponseHeaders");
                    responseHeaders["content-length"] = new string[] { "10" };

                    responseStream.Write(new byte[10], 0, 10);
                    return TaskHelpers.Completed();
                },
                HttpServerAddress, null);

            using (listener)
            {
                listener.Start();
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(HttpClientAddress);
                Assert.AreEqual(HttpStatusCode.SwitchingProtocols, response.StatusCode);
                Assert.AreEqual("Switching Protocols", response.ReasonPhrase);
                Assert.AreEqual(2, response.Headers.Count()); // Date, Server
                Assert.IsTrue(response.Headers.Date.HasValue);
                Assert.AreEqual(1, response.Headers.Server.Count);
                Assert.AreEqual(0, (await response.Content.ReadAsByteArrayAsync()).Length);
            }
        }

        [TestMethod]
        public async Task OwinHttpListenerResponse_OnFirstWrite_OnSendingHeaders()
        {
            OwinHttpListener listener = new OwinHttpListener(
                env =>
                {
                    env["owin.ResponseStatusCode"] = 200;
                    env["owin.ResponseReasonPhrase"] = "Custom";
                    Stream responseStream = env.Get<Stream>("owin.ResponseBody");

                    var responseHeaders = env.Get<IDictionary<string, string[]>>("owin.ResponseHeaders");

                    env.Get<Action<Action<object>, object>>("server.OnSendingHeaders")(state => responseHeaders["custom-header"] = new string[] { "customvalue" }, null);

                    responseHeaders["content-length"] = new string[] { "10" };

                    responseStream.Write(new byte[10], 0, 10);

                    return TaskHelpers.Completed();
                },
                HttpServerAddress, null);

            using (listener)
            {
                listener.Start();
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(HttpClientAddress);
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                Assert.AreEqual("Custom", response.ReasonPhrase);
                Assert.AreEqual(3, response.Headers.Count()); // Date, Server
                Assert.IsTrue(response.Headers.Date.HasValue);
                Assert.AreEqual(1, response.Headers.Server.Count);
                Assert.AreEqual("customvalue", response.Headers.GetValues("custom-header").First());
                Assert.AreEqual(10, (await response.Content.ReadAsByteArrayAsync()).Length);
            }
        }

        [TestMethod]
        public async Task OwinHttpListenerResponse_NoWrite_OnSendingHeaders()
        {
            OwinHttpListener listener = new OwinHttpListener(
                env =>
                {
                    env["owin.ResponseStatusCode"] = 200;
                    env["owin.ResponseReasonPhrase"] = "Custom";
                    Stream responseStream = env.Get<Stream>("owin.ResponseBody");

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
                HttpServerAddress, null);

            using (listener)
            {
                listener.Start();
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(HttpClientAddress);
                Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
                Assert.AreEqual("Custom1", response.ReasonPhrase);
                Assert.AreEqual(3, response.Headers.Count()); // Date, Server
                Assert.IsTrue(response.Headers.Date.HasValue);
                Assert.AreEqual(1, response.Headers.Server.Count);
                Assert.AreEqual("customvalue", response.Headers.GetValues("custom-header").First());
                Assert.AreEqual(0, (await response.Content.ReadAsByteArrayAsync()).Length);
            }
        }
    }
}
