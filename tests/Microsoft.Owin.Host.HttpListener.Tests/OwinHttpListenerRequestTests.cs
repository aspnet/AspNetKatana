// <copyright file="OwinHttpListenerRequestTests.cs" company="Katana contributors">
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
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Owin.Host.HttpListener.Tests
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    using HttpListener = System.Net.HttpListener;

    /// These tests measure the results of the OwinHttpListenerRequest construction as presented through the OWIN interface.
    /// NOTE: These tests require SetupProject.bat to be run as admin from a VS command prompt once per machine.
    public class OwinHttpListenerRequestTests
    {
        private static readonly string[] HttpServerAddress = new string[] { "http", "+", "8080", "/BaseAddress/" };
        private const string HttpClientAddress = "http://localhost:8080/BaseAddress/";
        private static readonly string[] HttpsServerAddress = new string[] { "https", "+", "9090", "/BaseAddress/" };
        private const string HttpsClientAddress = "https://localhost:9090/BaseAddress/";

        [Fact]
        public async Task CallParameters_EmptyGetRequest_NullBodyNonNullCollections()
        {
            var listener = CreateServer(
                env =>
                {
                    Assert.NotNull(env);
                    Assert.NotNull(env.Get<Stream>("owin.RequestBody"));
                    Assert.NotNull(env.Get<Stream>("owin.ResponseBody"));
                    Assert.NotNull(env.Get<IDictionary<string, string[]>>("owin.RequestHeaders"));
                    Assert.NotNull(env.Get<IDictionary<string, string[]>>("owin.ResponseHeaders"));
                    return TaskHelpers.Completed();
                },
                HttpServerAddress);

            await SendGetRequest(listener, HttpClientAddress);
        }

        [Fact]
        public async Task Environment_EmptyGetRequest_RequiredKeysPresentAndCorrect()
        {
            var listener = CreateServer(
                env =>
                {
                    object ignored;
                    Assert.True(env.TryGetValue("owin.RequestMethod", out ignored));
                    Assert.Equal("GET", env["owin.RequestMethod"]);

                    Assert.True(env.TryGetValue("owin.RequestPath", out ignored));
                    Assert.Equal("/SubPath", env["owin.RequestPath"]);

                    Assert.True(env.TryGetValue("owin.RequestPathBase", out ignored));
                    Assert.Equal("/BaseAddress", env["owin.RequestPathBase"]);

                    Assert.True(env.TryGetValue("owin.RequestProtocol", out ignored));
                    Assert.Equal("HTTP/1.1", env["owin.RequestProtocol"]);

                    Assert.True(env.TryGetValue("owin.RequestQueryString", out ignored));
                    Assert.Equal("QueryString", env["owin.RequestQueryString"]);

                    Assert.True(env.TryGetValue("owin.RequestScheme", out ignored));
                    Assert.Equal("http", env["owin.RequestScheme"]);

                    Assert.True(env.TryGetValue("owin.Version", out ignored));
                    Assert.Equal("1.0", env["owin.Version"]);

                    return TaskHelpers.Completed();
                },
                HttpServerAddress);

            await SendGetRequest(listener, HttpClientAddress + "SubPath?QueryString");
        }

        [Fact]
        public async Task Environment_Post10Request_ExpectedKeyValueChanges()
        {
            var listener = CreateServer(
                env =>
                {
                    object ignored;
                    Assert.True(env.TryGetValue("owin.RequestMethod", out ignored));
                    Assert.Equal("POST", (string)env["owin.RequestMethod"]);

                    Assert.True(env.TryGetValue("owin.RequestPath", out ignored));
                    Assert.Equal("/SubPath", (string)env["owin.RequestPath"]);

                    Assert.True(env.TryGetValue("owin.RequestPathBase", out ignored));
                    Assert.Equal("/BaseAddress", (string)env["owin.RequestPathBase"]);

                    Assert.True(env.TryGetValue("owin.RequestProtocol", out ignored));
                    Assert.Equal("HTTP/1.0", (string)env["owin.RequestProtocol"]);

                    Assert.True(env.TryGetValue("owin.RequestQueryString", out ignored));
                    Assert.Equal("QueryString", (string)env["owin.RequestQueryString"]);

                    Assert.True(env.TryGetValue("owin.RequestScheme", out ignored));
                    Assert.Equal("http", (string)env["owin.RequestScheme"]);

                    Assert.True(env.TryGetValue("owin.Version", out ignored));
                    Assert.Equal("1.0", (string)env["owin.Version"]);

                    return TaskHelpers.Completed();
                },
                HttpServerAddress);

            var request = new HttpRequestMessage(HttpMethod.Post, HttpClientAddress + "SubPath?QueryString");
            request.Content = new StringContent("Hello World");
            request.Version = new Version(1, 0);
            await SendRequest(listener, request);
        }

        [Fact]
        public async Task Headers_EmptyGetRequest_RequiredHeadersPresentAndCorrect()
        {
            var listener = CreateServer(
                env =>
                {
                    var requestHeaders = env.Get<IDictionary<string, string[]>>("owin.RequestHeaders");

                    string[] values;
                    Assert.True(requestHeaders.TryGetValue("host", out values));
                    Assert.Equal(1, values.Length);
                    Assert.Equal("localhost:8080", values[0]);

                    return TaskHelpers.Completed();
                },
                HttpServerAddress);

            await SendGetRequest(listener, HttpClientAddress);
        }

        [Fact]
        public async Task Headers_PostContentLengthRequest_RequiredHeadersPresentAndCorrect()
        {
            string requestBody = "Hello World";

            var listener = CreateServer(
                env =>
                {
                    var requestHeaders = env.Get<IDictionary<string, string[]>>("owin.RequestHeaders");

                    string[] values;

                    Assert.True(requestHeaders.TryGetValue("host", out values));
                    Assert.Equal(1, values.Length);
                    Assert.Equal("localhost:8080", values[0]);

                    Assert.True(requestHeaders.TryGetValue("Content-length", out values));
                    Assert.Equal(1, values.Length);
                    Assert.Equal(requestBody.Length.ToString(), values[0]);

                    Assert.True(requestHeaders.TryGetValue("exPect", out values));
                    Assert.Equal(1, values.Length);
                    Assert.Equal("100-continue", values[0]);

                    Assert.True(requestHeaders.TryGetValue("Content-Type", out values));
                    Assert.Equal(1, values.Length);
                    Assert.Equal("text/plain; charset=utf-8", values[0]);

                    return TaskHelpers.Completed();
                },
                HttpServerAddress);

            var request = new HttpRequestMessage(HttpMethod.Post, HttpClientAddress + "SubPath?QueryString");
            request.Content = new StringContent(requestBody);
            await SendRequest(listener, request);
        }

        [Fact]
        public async Task Headers_PostChunkedRequest_RequiredHeadersPresentAndCorrect()
        {
            string requestBody = "Hello World";

            var listener = CreateServer(
                env =>
                {
                    var requestHeaders = env.Get<IDictionary<string, string[]>>("owin.RequestHeaders");

                    string[] values;

                    Assert.True(requestHeaders.TryGetValue("host", out values));
                    Assert.Equal(1, values.Length);
                    Assert.Equal("localhost:8080", values[0]);

                    Assert.True(requestHeaders.TryGetValue("Transfer-encoding", out values));
                    Assert.Equal(1, values.Length);
                    Assert.Equal("chunked", values[0]);

                    Assert.True(requestHeaders.TryGetValue("exPect", out values));
                    Assert.Equal(1, values.Length);
                    Assert.Equal("100-continue", values[0]);

                    Assert.True(requestHeaders.TryGetValue("Content-Type", out values));
                    Assert.Equal(1, values.Length);
                    Assert.Equal("text/plain; charset=utf-8", values[0]);

                    return TaskHelpers.Completed();
                },
                HttpServerAddress);

            var request = new HttpRequestMessage(HttpMethod.Post, HttpClientAddress + "SubPath?QueryString");
            request.Headers.TransferEncodingChunked = true;
            request.Content = new StringContent(requestBody);
            await SendRequest(listener, request);
        }

        [Fact]
        public async Task Body_PostContentLengthZero_NullStream()
        {
            var listener = CreateServer(
                env =>
                {
                    string[] values;
                    var requestHeaders = env.Get<IDictionary<string, string[]>>("owin.RequestHeaders");

                    Assert.True(requestHeaders.TryGetValue("Content-length", out values));
                    Assert.Equal(1, values.Length);
                    Assert.Equal("0", values[0]);

                    Assert.NotNull(env.Get<Stream>("owin.RequestBody"));

                    return TaskHelpers.Completed();
                },
                HttpServerAddress);

            var request = new HttpRequestMessage(HttpMethod.Post, HttpClientAddress);
            request.Content = new StringContent(string.Empty);
            await SendRequest(listener, request);
        }

        [Fact]
        public async Task Body_PostContentLengthX_StreamWithXBytes()
        {
            var listener = CreateServer(
                env =>
                {
                    string[] values;
                    var requestHeaders = env.Get<IDictionary<string, string[]>>("owin.RequestHeaders");

                    Assert.True(requestHeaders.TryGetValue("Content-length", out values));
                    Assert.Equal(1, values.Length);
                    Assert.Equal("11", values[0]);

                    var requestBody = env.Get<Stream>("owin.RequestBody");
                    Assert.NotNull(requestBody);

                    var buffer = new MemoryStream();
                    requestBody.CopyTo(buffer);
                    Assert.Equal(11, buffer.Length);

                    return TaskHelpers.Completed();
                },
                HttpServerAddress);

            var request = new HttpRequestMessage(HttpMethod.Post, HttpClientAddress);
            request.Content = new StringContent("Hello World");
            await SendRequest(listener, request);
        }

        [Fact]
        public async Task Body_PostChunkedEmpty_StreamWithZeroBytes()
        {
            var listener = CreateServer(
                env =>
                {
                    string[] values;
                    var requestHeaders = env.Get<IDictionary<string, string[]>>("owin.RequestHeaders");

                    Assert.True(requestHeaders.TryGetValue("Transfer-Encoding", out values));
                    Assert.Equal(1, values.Length);
                    Assert.Equal("chunked", values[0]);

                    var requestBody = env.Get<Stream>("owin.RequestBody");
                    Assert.NotNull(requestBody);

                    var buffer = new MemoryStream();
                    requestBody.CopyTo(buffer);
                    Assert.Equal(0, buffer.Length);

                    return TaskHelpers.Completed();
                },
                HttpServerAddress);

            var request = new HttpRequestMessage(HttpMethod.Post, HttpClientAddress);
            request.Headers.TransferEncodingChunked = true;
            request.Content = new StringContent(string.Empty);
            await SendRequest(listener, request);
        }

        [Fact]
        public async Task Body_PostChunkedX_StreamWithXBytes()
        {
            var listener = CreateServer(
                env =>
                {
                    string[] values;
                    var requestHeaders = env.Get<IDictionary<string, string[]>>("owin.RequestHeaders");

                    Assert.True(requestHeaders.TryGetValue("Transfer-Encoding", out values));
                    Assert.Equal(1, values.Length);
                    Assert.Equal("chunked", values[0]);

                    var requestBody = env.Get<Stream>("owin.RequestBody");
                    Assert.NotNull(requestBody);

                    var buffer = new MemoryStream();
                    requestBody.CopyTo(buffer);
                    Assert.Equal(11, buffer.Length);

                    return TaskHelpers.Completed();
                },
                HttpServerAddress);

            var request = new HttpRequestMessage(HttpMethod.Post, HttpClientAddress);
            request.Headers.TransferEncodingChunked = true;
            request.Content = new StringContent("Hello World");
            await SendRequest(listener, request);
        }

        private OwinHttpListener CreateServer(AppFunc app, string[] addressParts)
        {
            OwinHttpListener wrapper = new OwinHttpListener();
            wrapper.Start(wrapper.Listener, app, CreateAddress(addressParts), null);
            return wrapper;
        }

        private static IList<IDictionary<string, object>> CreateAddress(string[] addressParts)
        {
            Dictionary<string, object> address = new Dictionary<string, object>();
            address["scheme"] = addressParts[0];
            address["host"] = addressParts[1];
            address["port"] = addressParts[2];
            address["path"] = addressParts[3];

            IList<IDictionary<string, object>> list = new List<IDictionary<string, object>>();
            list.Add(address);
            return list;
        }

        private async Task SendGetRequest(OwinHttpListener listener, string address)
        {
            using (listener)
            {
                var client = new HttpClient();
                string result = await client.GetStringAsync(address);
            }
        }

        private async Task SendRequest(OwinHttpListener listener, HttpRequestMessage request)
        {
            using (listener)
            {
                var client = new HttpClient();
                HttpResponseMessage result = await client.SendAsync(request);
                result.EnsureSuccessStatusCode();
            }
        }
    }
}
