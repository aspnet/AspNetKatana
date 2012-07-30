//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Katana.Server.HttpListener.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Katana.Server.HttpListenerWrapper;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Owin;

    /// NOTE: These tests require SetupProject.bat to be run as admin from a VS command prompt once per machine.
    [TestClass]
    public class OwinHttpListenerResponseTests
    {
        private const string HttpServerAddress = "http://*:8080/BaseAddress/";
        private const string HttpClientAddress = "http://localhost:8080/BaseAddress/";
        private const string HttpsServerAddress = "https://*:9090/BaseAddress/";
        private const string HttpsClientAddress = "https://localhost:9090/BaseAddress/";

        [TestMethod]
        public async Task OwinHttpListenerResponse_Empty200Response_Success()
        {
            OwinHttpListener listener = new OwinHttpListener(call => this.CreateEmptyResponseTask(200), HttpServerAddress);

            using (listener)
            {
                listener.Start(1);
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
                call =>
                {
                    ResultParameters results = this.CreateEmptyResponse(200);
                    results.Headers = null;
                    return Task.FromResult(results);
                }, 
                HttpServerAddress);

            using (listener)
            {
                listener.Start(1);
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(HttpClientAddress);
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            }
        }

        [TestMethod]
        public async Task ResultParmeters_NullPropertiesDictionary_SucceedAnyways()
        {
            OwinHttpListener listener = new OwinHttpListener(
                call =>
                {
                    ResultParameters results = this.CreateEmptyResponse(200);
                    results.Properties = null;
                    return Task.FromResult(results);
                }, 
                HttpServerAddress);

            using (listener)
            {
                listener.Start(1);
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(HttpClientAddress);
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            }
        }

        [TestMethod]
        public async Task Headers_CustomHeaders_PassedThrough()
        {
            OwinHttpListener listener = new OwinHttpListener(
                call =>
                {
                    ResultParameters results = this.CreateEmptyResponse(200);
                    results.Headers.Add("Custom1", new string[] { "value1a", "value1b" });
                    results.Headers.Add("Custom2", new string[] { "value2a, value2b" });
                    results.Headers.Add("Custom3", new string[] { "value3a, value3b", "value3c" });
                    return Task.FromResult(results);
                }, 
                HttpServerAddress);

            using (listener)
            {
                listener.Start(1);
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
                call =>
                {
                    ResultParameters results = this.CreateEmptyResponse(200);
                    results.Properties.Add("owin.ResponseProtocol", "HTTP/1.0");
                    results.Headers.Add("KEEP-alive", new string[] { "TRUE" });
                    results.Headers.Add("content-length", new string[] { "0" });
                    results.Headers.Add("www-Authenticate", new string[] { "Basic", "NTLM" });
                    return Task.FromResult(results);
                }, 
                HttpServerAddress);

            using (listener)
            {
                listener.Start(1);
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
                call =>
                {
                    ResultParameters results = this.CreateEmptyResponse(200);
                    results.Headers.Add("Transfer-Encoding", new string[] { "ChUnKed" });
                    results.Headers.Add("CONNECTION", new string[] { "ClOsE" });
                    return Task.FromResult(results);
                }, 
                HttpServerAddress);

            using (listener)
            {
                listener.Start(1);
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
        [ExpectedException(typeof(HttpRequestException))]
        public async Task Headers_BadContentLength_ConnectionClosed()
        {
            OwinHttpListener listener = new OwinHttpListener(
                call =>
                {
                    ResultParameters results = this.CreateEmptyResponse(200);
                    results.Headers.Add("content-length", new string[] { "-10" });
                    return Task.FromResult(results);
                }, 
                HttpServerAddress);

            using (listener)
            {
                listener.Start(1);
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(HttpClientAddress);
            }
        }

        [TestMethod]
        public async Task Properties_CustomReasonPhrase_PassedThrough()
        {
            OwinHttpListener listener = new OwinHttpListener(
                call =>
                {
                    ResultParameters results = this.CreateEmptyResponse(200);
                    results.Properties.Add("owin.ReasonPhrase", "Awesome");
                    return Task.FromResult(results);
                }, 
                HttpServerAddress);

            using (listener)
            {
                listener.Start(1);
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(HttpClientAddress);
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                Assert.AreEqual("Awesome", response.ReasonPhrase);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpRequestException))]
        public async Task Properties_BadReasonPhrase_ConnectionClosed()
        {
            OwinHttpListener listener = new OwinHttpListener(
                call =>
                {
                    ResultParameters results = this.CreateEmptyResponse(200);
                    results.Properties.Add("owin.ReasonPhrase", int.MaxValue);
                    return Task.FromResult(results);
                }, 
                HttpServerAddress);

            using (listener)
            {
                listener.Start(1);
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(HttpClientAddress);
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            }
        }

        // Note that Http.Sys does not allow HTTP/1.0 responses.
        [TestMethod]
        public async Task Properties_HTTP10Protocol_NotPassedThrough()
        {
            OwinHttpListener listener = new OwinHttpListener(
                call =>
                {
                    ResultParameters results = this.CreateEmptyResponse(200);
                    results.Properties.Add("owin.ResponseProtocol", "http/1.0");
                    return Task.FromResult(results);
                }, 
                HttpServerAddress);

            using (listener)
            {
                listener.Start(1);
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(HttpClientAddress);
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                Assert.AreEqual(new Version(1, 1), response.Version);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpRequestException))]
        public async Task Properties_UnknownProtocol_ConnectionClosed()
        {
            OwinHttpListener listener = new OwinHttpListener(
                call =>
                {
                    ResultParameters results = this.CreateEmptyResponse(200);
                    results.Properties.Add("owin.ResponseProtocol", "http/2.0");
                    return Task.FromResult(results);
                }, 
                HttpServerAddress);

            using (listener)
            {
                listener.Start(1);
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(HttpClientAddress);
            }
        }

        [TestMethod]
        public async Task Body_SmallChunked_Success()
        {
            OwinHttpListener listener = new OwinHttpListener(
                call =>
                {
                    ResultParameters results = this.CreateEmptyResponse(200);
                    results.Body = stream =>
                    {
                        stream.Write(new byte[10], 0, 10);
                        return Task.FromResult<object>(null);
                    };
                    return Task.FromResult(results);
                }, 
                HttpServerAddress);

            using (listener)
            {
                listener.Start(1);
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
                call =>
                {
                    ResultParameters results = this.CreateEmptyResponse(200);
                    results.Body = async stream =>
                    {
                        for (int i = 0; i < 100; i++)
                        {
                            await stream.WriteAsync(new byte[1000], 0, 1000);
                        }
                    };
                    return Task.FromResult(results);
                }, 
                HttpServerAddress);

            using (listener)
            {
                listener.Start(1);
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
                call =>
                {
                    ResultParameters results = this.CreateEmptyResponse(200);
                    results.Headers.Add("Content-Length", new string[] { "100" });
                    results.Body = stream =>
                    {
                        stream.Write(new byte[95], 0, 95);
                        return Task.FromResult<object>(null);
                    };
                    return Task.FromResult(results);
                }, 
                HttpServerAddress);

            using (listener)
            {
                listener.Start(1);
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
                call =>
                {
                    ResultParameters results = this.CreateEmptyResponse(200);
                    results.Headers.Add("Content-Length", new string[] { "100" });
                    results.Body = stream =>
                    {
                        stream.Write(new byte[105], 0, 105);
                        return Task.FromResult<object>(null);
                    };
                    return Task.FromResult(results);
                }, 
                HttpServerAddress);

            using (listener)
            {
                listener.Start(1);
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(HttpClientAddress);
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                Assert.AreEqual(105, (await response.Content.ReadAsByteArrayAsync()).Length);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpRequestException))]
        public async Task EndToEnd_AppReturns100Continue_ConnectionClosed()
        {
            OwinHttpListener listener = new OwinHttpListener(call => this.CreateEmptyResponseTask(100), HttpServerAddress);

            using (listener)
            {
                listener.Start(1);
                HttpClient client = new HttpClient();
                string dataString = "Hello World";
                HttpResponseMessage response = await client.PostAsync(HttpClientAddress, new StringContent(dataString));
            }
        }

        [TestMethod]
        public async Task OwinHttpListenerResponse_Empty101Response_Success()
        {
            OwinHttpListener listener = new OwinHttpListener(call => this.CreateEmptyResponseTask(101), HttpServerAddress);

            using (listener)
            {
                listener.Start(1);
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
            bool bodyInvoked = false;
            OwinHttpListener listener = new OwinHttpListener(
                call =>
                {
                    ResultParameters result = this.CreateEmptyResponse(101);
                    result.Headers.Add("Content-Length", new string[] { "10" });
                    result.Body = stream =>
                    {
                        bodyInvoked = true;
                        stream.Write(new byte[10], 0, 10);
                        return Task.FromResult<object>(null);
                    };
                    return Task.FromResult(result);
                }, 
                HttpServerAddress);

            using (listener)
            {
                listener.Start(1);
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(HttpClientAddress);
                Assert.AreEqual(HttpStatusCode.SwitchingProtocols, response.StatusCode);
                Assert.AreEqual("Switching Protocols", response.ReasonPhrase);
                Assert.AreEqual(2, response.Headers.Count()); // Date, Server
                Assert.IsTrue(response.Headers.Date.HasValue);
                Assert.AreEqual(1, response.Headers.Server.Count);
                Assert.AreEqual(0, (await response.Content.ReadAsByteArrayAsync()).Length);
                Assert.IsTrue(bodyInvoked);
            }
        }

        private ResultParameters CreateEmptyResponse(int statusCode)
        {
            return new ResultParameters()
            {
                Headers = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase),
                Status = statusCode,
                Properties = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase),
                Body = null
            };
        }

        private Task<ResultParameters> CreateEmptyResponseTask(int statusCode)
        {
            return Task.FromResult(this.CreateEmptyResponse(statusCode));
        }
    }
}
