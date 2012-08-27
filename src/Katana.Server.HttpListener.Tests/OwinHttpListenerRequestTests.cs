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

    // TODO: Convert to XUnit?

    /// These tests measure the results of the OwinHttpListenerRequest construction as presented through the OWIN interface.
    /// NOTE: These tests require SetupProject.bat to be run as admin from a VS command prompt once per machine.
    [TestClass]
    public class OwinHttpListenerRequestTests
    {
        private static readonly string[] HttpServerAddress = new string[] { "http://*:8080/BaseAddress/" };
        private const string HttpClientAddress = "http://localhost:8080/BaseAddress/";
        private static readonly string[] HttpsServerAddress = new string[] { "https://*:9090/BaseAddress/" };
        private const string HttpsClientAddress = "https://localhost:9090/BaseAddress/";

        [TestMethod]
        public async Task CallParameters_EmptyGetRequest_NullBodyNonNullCollections()
        {
            OwinHttpListener listener = new OwinHttpListener(
                env => 
                {
                    Assert.IsNotNull(env);
                    Assert.IsNotNull(env.Get<Stream>("owin.RequestBody"));
                    Assert.IsNotNull(env.Get<Stream>("owin.ResponseBody"));
                    Assert.IsNotNull(env.Get<IDictionary<string, string[]>>("owin.RequestHeaders"));
                    Assert.IsNotNull(env.Get<IDictionary<string, string[]>>("owin.ResponseHeaders"));
                    return TaskHelpers.Completed();
                }, 
                HttpServerAddress);

            await this.SendGetRequest(listener, HttpClientAddress);
        }

        [TestMethod]
        public async Task Environment_EmptyGetRequest_RequiredKeysPresentAndCorrect()
        {
            OwinHttpListener listener = new OwinHttpListener(
                env =>
                {
                    object ignored;
                    Assert.IsTrue(env.TryGetValue("owin.RequestMethod", out ignored));
                    Assert.AreEqual("GET", (string)env["owin.RequestMethod"]);

                    Assert.IsTrue(env.TryGetValue("owin.RequestPath", out ignored));
                    Assert.AreEqual("/SubPath", (string)env["owin.RequestPath"]);

                    Assert.IsTrue(env.TryGetValue("owin.RequestPathBase", out ignored));
                    Assert.AreEqual("/BaseAddress", (string)env["owin.RequestPathBase"]);

                    Assert.IsTrue(env.TryGetValue("owin.RequestProtocol", out ignored));
                    Assert.AreEqual("HTTP/1.1", (string)env["owin.RequestProtocol"]);

                    Assert.IsTrue(env.TryGetValue("owin.RequestQueryString", out ignored));
                    Assert.AreEqual("QueryString", (string)env["owin.RequestQueryString"]);

                    Assert.IsTrue(env.TryGetValue("owin.RequestScheme", out ignored));
                    Assert.AreEqual("http", (string)env["owin.RequestScheme"]);
                    
                    Assert.IsTrue(env.TryGetValue("owin.Version", out ignored));
                    Assert.AreEqual("1.0", (string)env["owin.Version"]);

                    return TaskHelpers.Completed();
                },
                HttpServerAddress);

            await this.SendGetRequest(listener, HttpClientAddress + "SubPath?QueryString");
        }

        [TestMethod]
        public async Task Environment_Post10Request_ExpectedKeyValueChanges()
        {
            OwinHttpListener listener = new OwinHttpListener(
                env =>
                {
                    object ignored;
                    Assert.IsTrue(env.TryGetValue("owin.RequestMethod", out ignored));
                    Assert.AreEqual("POST", (string)env["owin.RequestMethod"]);

                    Assert.IsTrue(env.TryGetValue("owin.RequestPath", out ignored));
                    Assert.AreEqual("/SubPath", (string)env["owin.RequestPath"]);

                    Assert.IsTrue(env.TryGetValue("owin.RequestPathBase", out ignored));
                    Assert.AreEqual("/BaseAddress", (string)env["owin.RequestPathBase"]);

                    Assert.IsTrue(env.TryGetValue("owin.RequestProtocol", out ignored));
                    Assert.AreEqual("HTTP/1.0", (string)env["owin.RequestProtocol"]);

                    Assert.IsTrue(env.TryGetValue("owin.RequestQueryString", out ignored));
                    Assert.AreEqual("QueryString", (string)env["owin.RequestQueryString"]);

                    Assert.IsTrue(env.TryGetValue("owin.RequestScheme", out ignored));
                    Assert.AreEqual("http", (string)env["owin.RequestScheme"]);

                    Assert.IsTrue(env.TryGetValue("owin.Version", out ignored));
                    Assert.AreEqual("1.0", (string)env["owin.Version"]);

                    return TaskHelpers.Completed();
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
                env =>
                {
                    var requestHeaders = env.Get<IDictionary<string, string[]>>("owin.RequestHeaders");
                    Assert.AreEqual(1, requestHeaders.Count);

                    string[] values;
                    Assert.IsTrue(requestHeaders.TryGetValue("host", out values));
                    Assert.AreEqual(1, values.Length);
                    Assert.AreEqual("localhost:8080", values[0]);

                    return TaskHelpers.Completed();
                },
                HttpServerAddress);

            await this.SendGetRequest(listener, HttpClientAddress);
        }

        [TestMethod]
        public async Task Headers_PostContentLengthRequest_RequiredHeadersPresentAndCorrect()
        {
            string requestBody = "Hello World";

            OwinHttpListener listener = new OwinHttpListener(
                env =>
                {
                    var requestHeaders = env.Get<IDictionary<string, string[]>>("owin.RequestHeaders");
                    Assert.AreEqual(4, requestHeaders.Count);

                    string[] values;

                    Assert.IsTrue(requestHeaders.TryGetValue("host", out values));
                    Assert.AreEqual(1, values.Length);
                    Assert.AreEqual("localhost:8080", values[0]);

                    Assert.IsTrue(requestHeaders.TryGetValue("Content-length", out values));
                    Assert.AreEqual(1, values.Length);
                    Assert.AreEqual(requestBody.Length.ToString(), values[0]);

                    Assert.IsTrue(requestHeaders.TryGetValue("exPect", out values));
                    Assert.AreEqual(1, values.Length);
                    Assert.AreEqual("100-continue", values[0]);

                    Assert.IsTrue(requestHeaders.TryGetValue("Content-Type", out values));
                    Assert.AreEqual(1, values.Length);
                    Assert.AreEqual("text/plain; charset=utf-8", values[0]);

                    return TaskHelpers.Completed();
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
                env =>
                {
                    var requestHeaders = env.Get<IDictionary<string, string[]>>("owin.RequestHeaders");
                    Assert.AreEqual(4, requestHeaders.Count);

                    string[] values;

                    Assert.IsTrue(requestHeaders.TryGetValue("host", out values));
                    Assert.AreEqual(1, values.Length);
                    Assert.AreEqual("localhost:8080", values[0]);

                    Assert.IsTrue(requestHeaders.TryGetValue("Transfer-encoding", out values));
                    Assert.AreEqual(1, values.Length);
                    Assert.AreEqual("chunked", values[0]);

                    Assert.IsTrue(requestHeaders.TryGetValue("exPect", out values));
                    Assert.AreEqual(1, values.Length);
                    Assert.AreEqual("100-continue", values[0]);

                    Assert.IsTrue(requestHeaders.TryGetValue("Content-Type", out values));
                    Assert.AreEqual(1, values.Length);
                    Assert.AreEqual("text/plain; charset=utf-8", values[0]);

                    return TaskHelpers.Completed();
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
                   env =>
                   {
                       string[] values;
                       var requestHeaders = env.Get<IDictionary<string, string[]>>("owin.RequestHeaders");

                       Assert.IsTrue(requestHeaders.TryGetValue("Content-length", out values));
                       Assert.AreEqual(1, values.Length);
                       Assert.AreEqual("0", values[0]);

                       Assert.IsNotNull(env.Get<Stream>("owin.RequestBody"));

                       return TaskHelpers.Completed();
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
                   env =>
                   {
                       string[] values;
                       var requestHeaders = env.Get<IDictionary<string, string[]>>("owin.RequestHeaders");

                       Assert.IsTrue(requestHeaders.TryGetValue("Content-length", out values));
                       Assert.AreEqual(1, values.Length);
                       Assert.AreEqual("11", values[0]);

                       var requestBody = env.Get<Stream>("owin.RequestBody");
                       Assert.IsNotNull(requestBody);

                       MemoryStream buffer = new MemoryStream();
                       requestBody.CopyTo(buffer);
                       Assert.AreEqual(11, buffer.Length);

                       return TaskHelpers.Completed();
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
                   env =>
                   {
                       string[] values;
                       var requestHeaders = env.Get<IDictionary<string, string[]>>("owin.RequestHeaders");

                       Assert.IsTrue(requestHeaders.TryGetValue("Transfer-Encoding", out values));
                       Assert.AreEqual(1, values.Length);
                       Assert.AreEqual("chunked", values[0]);

                       var requestBody = env.Get<Stream>("owin.RequestBody");
                       Assert.IsNotNull(requestBody);

                       MemoryStream buffer = new MemoryStream();
                       requestBody.CopyTo(buffer);
                       Assert.AreEqual(0, buffer.Length);

                       return TaskHelpers.Completed();
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
                   env =>
                   {
                       string[] values;
                       var requestHeaders = env.Get<IDictionary<string, string[]>>("owin.RequestHeaders");

                       Assert.IsTrue(requestHeaders.TryGetValue("Transfer-Encoding", out values));
                       Assert.AreEqual(1, values.Length);
                       Assert.AreEqual("chunked", values[0]);

                       var requestBody = env.Get<Stream>("owin.RequestBody");
                       Assert.IsNotNull(requestBody);

                       MemoryStream buffer = new MemoryStream();
                       requestBody.CopyTo(buffer);
                       Assert.AreEqual(11, buffer.Length);

                       return TaskHelpers.Completed();
                   },
                   HttpServerAddress);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, HttpClientAddress);
            request.Headers.TransferEncodingChunked = true;
            request.Content = new StringContent("Hello World");
            await this.SendRequest(listener, request);
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
