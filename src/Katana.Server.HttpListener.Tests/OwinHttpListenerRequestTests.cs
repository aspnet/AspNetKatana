//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Katana.Server.HttpListener.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Katana.Server.HttpListenerWrapper;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Owin;

    /// These tests measure the results of the OwinHttpListenerRequest construction as presented through the OWIN interface.
    [TestClass]
    public class OwinHttpListenerRequestTests
    {
        private const string HttpServerAddress = "http://+:8080/BaseAddress/";
        private const string HttpClientAddress = "http://localhost:8080/BaseAddress/";
        private const string HttpsServerAddress = "https://+:9090/BaseAddress/";
        private const string HttpsClientAddress = "https://localhost:8080/BaseAddress/";

        [TestMethod]
        public async Task CallParameters_EmptyGetRequest_NullBodyNonNullCollections()
        {
            OwinHttpListener listener = new OwinHttpListener(
                (call, cancel) => 
                {
                    Assert.IsNull(call.Body);
                    Assert.IsNotNull(call.Environment);
                    Assert.IsNotNull(call.Headers);
                    Assert.IsFalse(cancel.IsCancellationRequested);

                    return this.CreateEmptyResponseTask(200);
                }, 
                HttpServerAddress);

            await this.SendGetRequest(listener, HttpClientAddress);
        }

        [TestMethod]
        public async Task Environment_EmptyGetRequest_RequiredKeysPresentAndCorrect()
        {
            OwinHttpListener listener = new OwinHttpListener(
                (call, cancel) =>
                {
                    object ignored;
                    Assert.IsTrue(call.Environment.TryGetValue("owin.RequestMethod", out ignored));
                    Assert.AreEqual("GET", (string)call.Environment["owin.RequestMethod"]);

                    Assert.IsTrue(call.Environment.TryGetValue("owin.RequestPath", out ignored));
                    Assert.AreEqual("/SubPath", (string)call.Environment["owin.RequestPath"]);

                    Assert.IsTrue(call.Environment.TryGetValue("owin.RequestPathBase", out ignored));
                    Assert.AreEqual("/BaseAddress", (string)call.Environment["owin.RequestPathBase"]);

                    Assert.IsTrue(call.Environment.TryGetValue("owin.RequestProtocol", out ignored));
                    Assert.AreEqual("HTTP/1.1", (string)call.Environment["owin.RequestProtocol"]);

                    Assert.IsTrue(call.Environment.TryGetValue("owin.RequestQueryString", out ignored));
                    Assert.AreEqual("QueryString", (string)call.Environment["owin.RequestQueryString"]);

                    Assert.IsTrue(call.Environment.TryGetValue("owin.RequestScheme", out ignored));
                    Assert.AreEqual("http", (string)call.Environment["owin.RequestScheme"]);
                    
                    Assert.IsTrue(call.Environment.TryGetValue("owin.RemoteHost", out ignored));
                    Assert.AreEqual("::1", (string)call.Environment["owin.RemoteHost"]);
                    
                    Assert.IsTrue(call.Environment.TryGetValue("owin.Version", out ignored));
                    Assert.AreEqual("1.0", (string)call.Environment["owin.Version"]);

                    return CreateEmptyResponseTask(200);
                },
                HttpServerAddress);

            await this.SendGetRequest(listener, HttpClientAddress + "SubPath?QueryString");
        }

        [TestMethod]
        public async Task Environment_Post10Request_ExpectedKeyValueChanges()
        {
            OwinHttpListener listener = new OwinHttpListener(
                (call, cancel) =>
                {
                    object ignored;
                    Assert.IsTrue(call.Environment.TryGetValue("owin.RequestMethod", out ignored));
                    Assert.AreEqual("POST", (string)call.Environment["owin.RequestMethod"]);

                    Assert.IsTrue(call.Environment.TryGetValue("owin.RequestPath", out ignored));
                    Assert.AreEqual("/SubPath", (string)call.Environment["owin.RequestPath"]);

                    Assert.IsTrue(call.Environment.TryGetValue("owin.RequestPathBase", out ignored));
                    Assert.AreEqual("/BaseAddress", (string)call.Environment["owin.RequestPathBase"]);

                    Assert.IsTrue(call.Environment.TryGetValue("owin.RequestProtocol", out ignored));
                    Assert.AreEqual("HTTP/1.0", (string)call.Environment["owin.RequestProtocol"]);

                    Assert.IsTrue(call.Environment.TryGetValue("owin.RequestQueryString", out ignored));
                    Assert.AreEqual("QueryString", (string)call.Environment["owin.RequestQueryString"]);

                    Assert.IsTrue(call.Environment.TryGetValue("owin.RequestScheme", out ignored));
                    Assert.AreEqual("http", (string)call.Environment["owin.RequestScheme"]);

                    Assert.IsTrue(call.Environment.TryGetValue("owin.RemoteHost", out ignored));
                    Assert.AreEqual("::1", (string)call.Environment["owin.RemoteHost"]);

                    Assert.IsTrue(call.Environment.TryGetValue("owin.Version", out ignored));
                    Assert.AreEqual("1.0", (string)call.Environment["owin.Version"]);

                    return CreateEmptyResponseTask(200);
                },
                HttpServerAddress);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, HttpClientAddress + "SubPath?QueryString");
            request.Content = new StringContent("Hello World");
            request.Version = new Version(1, 0);
            await this.SendRequest(listener, request);
        }

        [TestMethod]
        public async Task Headers_EmptyGetRequest_RequiredHeadersPresentAndCorrect()
        {
            OwinHttpListener listener = new OwinHttpListener(
                (call, cancel) =>
                {
                    Assert.AreEqual(1, call.Headers.Count);

                    string[] values;
                    Assert.IsTrue(call.Headers.TryGetValue("host", out values));
                    Assert.AreEqual(1, values.Length);
                    Assert.AreEqual("localhost:8080", values[0]);

                    return CreateEmptyResponseTask(200);
                },
                HttpServerAddress);

            await this.SendGetRequest(listener, HttpClientAddress);
        }

        [TestMethod]
        public async Task Headers_PostContentLengthRequest_RequiredHeadersPresentAndCorrect()
        {
            string requestBody = "Hello World";

            OwinHttpListener listener = new OwinHttpListener(
                (call, cancel) =>
                {
                    Assert.AreEqual(4, call.Headers.Count);

                    string[] values;

                    Assert.IsTrue(call.Headers.TryGetValue("host", out values));
                    Assert.AreEqual(1, values.Length);
                    Assert.AreEqual("localhost:8080", values[0]);

                    Assert.IsTrue(call.Headers.TryGetValue("Content-length", out values));
                    Assert.AreEqual(1, values.Length);
                    Assert.AreEqual(requestBody.Length.ToString(), values[0]);

                    Assert.IsTrue(call.Headers.TryGetValue("exPect", out values));
                    Assert.AreEqual(1, values.Length);
                    Assert.AreEqual("100-continue", values[0]);

                    Assert.IsTrue(call.Headers.TryGetValue("Content-Type", out values));
                    Assert.AreEqual(1, values.Length);
                    Assert.AreEqual("text/plain; charset=utf-8", values[0]);

                    return CreateEmptyResponseTask(200);
                },
                HttpServerAddress);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, HttpClientAddress + "SubPath?QueryString");
            request.Content = new StringContent(requestBody);
            await this.SendRequest(listener, request);
        }

        [TestMethod]
        public async Task Headers_PostChunkedRequest_RequiredHeadersPresentAndCorrect()
        {
            string requestBody = "Hello World";

            OwinHttpListener listener = new OwinHttpListener(
                (call, cancel) =>
                {
                    Assert.AreEqual(4, call.Headers.Count);

                    string[] values;

                    Assert.IsTrue(call.Headers.TryGetValue("host", out values));
                    Assert.AreEqual(1, values.Length);
                    Assert.AreEqual("localhost:8080", values[0]);

                    Assert.IsTrue(call.Headers.TryGetValue("Transfer-encoding", out values));
                    Assert.AreEqual(1, values.Length);
                    Assert.AreEqual("chunked", values[0]);

                    Assert.IsTrue(call.Headers.TryGetValue("exPect", out values));
                    Assert.AreEqual(1, values.Length);
                    Assert.AreEqual("100-continue", values[0]);

                    Assert.IsTrue(call.Headers.TryGetValue("Content-Type", out values));
                    Assert.AreEqual(1, values.Length);
                    Assert.AreEqual("text/plain; charset=utf-8", values[0]);

                    return CreateEmptyResponseTask(200);
                },
                HttpServerAddress);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, HttpClientAddress + "SubPath?QueryString");
            request.Headers.TransferEncodingChunked = true;
            request.Content = new StringContent(requestBody);
            await this.SendRequest(listener, request);
        }

        [TestMethod]
        public async Task Body_PostContentLengthZero_NullStream()
        {
            OwinHttpListener listener = new OwinHttpListener(
                   (call, cancel) =>
                   {
                       string[] values;

                       Assert.IsTrue(call.Headers.TryGetValue("Content-length", out values));
                       Assert.AreEqual(1, values.Length);
                       Assert.AreEqual("0", values[0]);

                       Assert.IsNull(call.Body);

                       return CreateEmptyResponseTask(200);
                   },
                   HttpServerAddress);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, HttpClientAddress);
            request.Content = new StringContent(string.Empty);
            await this.SendRequest(listener, request);
        }

        [TestMethod]
        public async Task Body_PostContentLengthX_StreamWithXBytes()
        {
            OwinHttpListener listener = new OwinHttpListener(
                   (call, cancel) =>
                   {
                       string[] values;

                       Assert.IsTrue(call.Headers.TryGetValue("Content-length", out values));
                       Assert.AreEqual(1, values.Length);
                       Assert.AreEqual("11", values[0]);

                       Assert.IsNotNull(call.Body);

                       MemoryStream buffer = new MemoryStream();
                       call.Body.CopyTo(buffer);
                       Assert.AreEqual(11, buffer.Length);

                       return CreateEmptyResponseTask(200);
                   },
                   HttpServerAddress);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, HttpClientAddress);
            request.Content = new StringContent("Hello World");
            await this.SendRequest(listener, request);
        }

        [TestMethod]
        public async Task Body_PostChunkedEmpty_StreamWithZeroBytes()
        {
            OwinHttpListener listener = new OwinHttpListener(
                   (call, cancel) =>
                   {
                       string[] values;

                       Assert.IsTrue(call.Headers.TryGetValue("Transfer-Encoding", out values));
                       Assert.AreEqual(1, values.Length);
                       Assert.AreEqual("chunked", values[0]);

                       Assert.IsNotNull(call.Body);

                       MemoryStream buffer = new MemoryStream();
                       call.Body.CopyTo(buffer);
                       Assert.AreEqual(0, buffer.Length);

                       return CreateEmptyResponseTask(200);
                   },
                   HttpServerAddress);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, HttpClientAddress);
            request.Headers.TransferEncodingChunked = true;
            request.Content = new StringContent(string.Empty);
            await this.SendRequest(listener, request);
        }

        [TestMethod]
        public async Task Body_PostChunkedX_StreamWithXBytes()
        {
            OwinHttpListener listener = new OwinHttpListener(
                   (call, cancel) =>
                   {
                       string[] values;

                       Assert.IsTrue(call.Headers.TryGetValue("Transfer-Encoding", out values));
                       Assert.AreEqual(1, values.Length);
                       Assert.AreEqual("chunked", values[0]);

                       Assert.IsNotNull(call.Body);

                       MemoryStream buffer = new MemoryStream();
                       call.Body.CopyTo(buffer);
                       Assert.AreEqual(11, buffer.Length);

                       return CreateEmptyResponseTask(200);
                   },
                   HttpServerAddress);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, HttpClientAddress);
            request.Headers.TransferEncodingChunked = true;
            request.Content = new StringContent("Hello World");
            await this.SendRequest(listener, request);
        }

        private Task<ResultParameters> CreateEmptyResponseTask(int statusCode)
        {
            ResultParameters results = new ResultParameters()
            {
                Headers = new Dictionary<string, string[]>(),
                Status = statusCode,
                Properties = new Dictionary<string, object>(),
                Body = null
            };

            return Task.FromResult(results);
        }

        private async Task SendGetRequest(OwinHttpListener listener, string address)
        {
            using (listener)
            {
                listener.Start(1);
                HttpClient client = new HttpClient();
                string result = await client.GetStringAsync(address);
            }
        }

        private async Task SendRequest(OwinHttpListener listener, HttpRequestMessage request)
        {
            using (listener)
            {
                listener.Start(1);
                HttpClient client = new HttpClient();
                HttpResponseMessage result = await client.SendAsync(request);
                result.EnsureSuccessStatusCode();
            }
        }
    }
}
