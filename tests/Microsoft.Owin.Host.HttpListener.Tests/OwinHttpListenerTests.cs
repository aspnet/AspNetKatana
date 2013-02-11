// <copyright file="OwinHttpListenerTests.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions;

namespace Microsoft.Owin.Host.HttpListener.Tests
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    // These tests measure that the core HttpListener wrapper functions as expected in normal and exceptional scenarios.
    // NOTE: These tests require SetupProject.bat to be run as admin from a VS command prompt once per machine.
    public class OwinHttpListenerTests
    {
        private static readonly string[] HttpServerAddress = new string[] { "http", "localhost", "8080", "/BaseAddress/" };
        private const string HttpClientAddress = "http://localhost:8080/BaseAddress/";
        private static readonly string[] HttpsServerAddress = new string[] { "https", "localhost", "9090", "/BaseAddress/" };
        private const string HttpsClientAddress = "https://localhost:9090/BaseAddress/";

        private readonly AppFunc _notImplemented = env => { throw new NotImplementedException(); };

        [Fact]
        public void OwinHttpListener_CreatedStartedStoppedDisposed_Success()
        {
            OwinHttpListener listener = CreateServer(_notImplemented, HttpServerAddress);
            using (listener)
            {
                listener.Stop();
            }
        }

        // HTTPS requires pre-configuring the server cert to work
        [Fact]
        public void OwinHttpListener_HttpsCreatedStartedStoppedDisposed_Success()
        {
            OwinHttpListener listener = CreateServer(_notImplemented, HttpsServerAddress);
            using (listener)
            {
                listener.Stop();
            }
        }

        [Fact]
        public void Ctor_PathMissingEndSlash_Added()
        {
            OwinHttpListener listener = CreateServer(_notImplemented, new string[]
            {
                "http", "localhost", "8080", "/BadPathDoesntEndInSlash"
            });
            using (listener)
            {
                listener.Stop();
            }
        }

        [Fact]
        public async Task EndToEnd_GetRequest_Success()
        {
            OwinHttpListener listener = CreateServer(env => TaskHelpers.Completed(), HttpServerAddress);
            HttpResponseMessage response = await SendGetRequest(listener, HttpClientAddress);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(0, response.Content.Headers.ContentLength.Value);
        }

        [Fact]
        public async Task EndToEnd_SingleThreadedTwoGetRequests_Success()
        {
            OwinHttpListener listener = CreateServer(env => TaskHelpers.Completed(), HttpServerAddress);
            using (listener)
            {
                var client = new HttpClient();
                string result = await client.GetStringAsync(HttpClientAddress);
                Assert.Equal(string.Empty, result);
                result = await client.GetStringAsync(HttpClientAddress);
                Assert.Equal(string.Empty, result);
            }
        }

        [Fact]
        public async Task EndToEnd_HttpsGetRequest_Success()
        {
            OwinHttpListener listener = CreateServer(
                async env =>
                {
                    object obj;
                    Assert.True(env.TryGetValue("ssl.LoadClientCertAsync", out obj));
                    Assert.NotNull(obj);
                    Assert.IsType(typeof(Func<Task>), obj);
                    var loadCert = (Func<Task>)obj;
                    await loadCert();
                    Assert.True(env.TryGetValue("ssl.ClientCertificate", out obj));
                    Assert.NotNull(obj);
                    Assert.IsType<X509Certificate2>(obj);
                },
                HttpsServerAddress);

            HttpResponseMessage response = await SendGetRequest(listener, HttpsClientAddress, ClientCertificateOption.Automatic);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(0, response.Content.Headers.ContentLength.Value);
        }

        [Fact]
        public async Task EndToEnd_HttpsGetRequestNoClientCert_Success()
        {
            OwinHttpListener listener = CreateServer(
                env =>
                {
                    object obj;
                    Assert.False(env.TryGetValue("owin.ClientCertificate", out obj));
                    return TaskHelpers.Completed();
                },
                HttpsServerAddress);

            HttpResponseMessage response = await SendGetRequest(listener, HttpsClientAddress, ClientCertificateOption.Manual);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(0, response.Content.Headers.ContentLength.Value);
        }

        [Fact]
        public async Task AppDelegate_ThrowsSync_500Error()
        {
            OwinHttpListener listener = CreateServer(_notImplemented, HttpServerAddress);
            HttpResponseMessage response = await SendGetRequest(listener, HttpClientAddress);
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Equal(0, response.Content.Headers.ContentLength.Value);
        }

        [Fact]
        public async Task AppDelegate_ReturnsExceptionAsync_500Error()
        {
            bool callCancelled = false;

            OwinHttpListener listener = CreateServer(
                async env =>
                {
                    GetCallCancelled(env).Register(() => callCancelled = true);
                    await Task.Delay(1);
                    throw new NotImplementedException();
                },
                HttpServerAddress);

            HttpResponseMessage response = await SendGetRequest(listener, HttpClientAddress);
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Equal(0, response.Content.Headers.ContentLength.Value);
            Assert.True(callCancelled);
        }

        [Fact]
        public async Task Body_PostEchoRequest_Success()
        {
            bool callCancelled = false;

            OwinHttpListener listener = CreateServer(
                async env =>
                {
                    GetCallCancelled(env).Register(() => callCancelled = true);
                    var requestHeaders = env.Get<IDictionary<string, string[]>>("owin.RequestHeaders");
                    var responseHeaders = env.Get<IDictionary<string, string[]>>("owin.ResponseHeaders");
                    responseHeaders.Add("Content-Length", requestHeaders["Content-Length"]);

                    var requestStream = env.Get<Stream>("owin.RequestBody");
                    var responseStream = env.Get<Stream>("owin.ResponseBody");

                    var buffer = new MemoryStream();
                    await requestStream.CopyToAsync(buffer, 1024);
                    buffer.Seek(0, SeekOrigin.Begin);
                    await buffer.CopyToAsync(responseStream, 1024);
                },
                HttpServerAddress);

            using (listener)
            {
                var client = new HttpClient();
                string dataString = "Hello World";
                HttpResponseMessage result = await client.PostAsync(HttpClientAddress, new StringContent(dataString));
                result.EnsureSuccessStatusCode();
                Assert.Equal(dataString.Length, result.Content.Headers.ContentLength.Value);
                Assert.Equal(dataString, await result.Content.ReadAsStringAsync());
                Assert.False(callCancelled);
            }
        }

        [Fact]
        public void BodyDelegate_ThrowsSync_ConnectionClosed()
        {
            bool callCancelled = false;
            OwinHttpListener listener = CreateServer(
                env =>
                {
                    GetCallCancelled(env).Register(() => callCancelled = true);
                    var responseHeaders = env.Get<IDictionary<string, string[]>>("owin.ResponseHeaders");
                    responseHeaders.Add("Content-Length", new string[] { "10" });

                    var responseStream = env.Get<Stream>("owin.ResponseBody");
                    responseStream.WriteByte(0xFF);

                    throw new NotImplementedException();
                },
                HttpServerAddress);

            try
            {
                // TODO: XUnit 2.0 adds support for Assert.Throws<...>(async () => await myTask);
                // that way we can specify the correct exception type.
                Assert.Throws<AggregateException>(() => SendGetRequest(listener, HttpClientAddress).Result);
            }
            finally
            {
                Assert.True(callCancelled);
            }
        }

        [Fact]
        public void BodyDelegate_ThrowsAsync_ConnectionClosed()
        {
            bool callCancelled = false;

            OwinHttpListener listener = CreateServer(
                env =>
                {
                    GetCallCancelled(env).Register(() => callCancelled = true);
                    var responseHeaders = env.Get<IDictionary<string, string[]>>("owin.ResponseHeaders");
                    responseHeaders.Add("Content-Length", new string[] { "10" });

                    var responseStream = env.Get<Stream>("owin.ResponseBody");
                    responseStream.WriteByte(0xFF);

                    return TaskHelpers.FromError(new NotImplementedException());
                },
                HttpServerAddress);

            try
            {
                Assert.Throws<AggregateException>(() => SendGetRequest(listener, HttpClientAddress).Result);
            }
            finally
            {
                Assert.True(callCancelled);
            }
        }

        [Theory]
        [InlineData("/", "", "", "/", "")]
        [InlineData("/path?query", "", "", "/path", "query")]
        [InlineData("/pathBase/path?query", "/pathBase", "/pathBase", "/path", "query")]
        public async Task PathAndQueryParsing_CorrectlySeperated(string clientString, string serverBasePath,
            string expectedBasePath, string expectedPath, string expectedQuery)
        {
            var serverAddress = new string[4];
            HttpServerAddress.CopyTo(serverAddress, 0);
            serverAddress[3] = serverBasePath;
            clientString = "http://localhost:8080" + clientString;

            OwinHttpListener listener = CreateServer(env =>
            {
                Assert.Equal(expectedBasePath, (string)env["owin.RequestPathBase"]);
                Assert.Equal(expectedPath, (string)env["owin.RequestPath"]);
                Assert.Equal(expectedQuery, (string)env["owin.RequestQueryString"]);
                return TaskHelpers.Completed();
            }, serverAddress);
            using (listener)
            {
                var client = new HttpClient();
                HttpResponseMessage result = await client.GetAsync(clientString);
                Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            }
        }

        [Theory]
        [InlineData("/", "", "", "/", "")]
        [InlineData("/path?query", "", "", "/path", "query")]
        [InlineData("/pathBase/path?query", "/pathBase", "/pathBase", "/path", "query")]
        [InlineData("/pathA/path?query", "/path", "", "/pathA/path", "query")]
        public async Task PathAndPathBase_CorrectlySeperated(string clientString, string serverBasePath,
            string expectedBasePath, string expectedPath, string expectedQuery)
        {
            var fallbackAddress = new string[4];
            HttpServerAddress.CopyTo(fallbackAddress, 0);
            fallbackAddress[3] = "/";
            var serverAddress = new string[4];
            HttpServerAddress.CopyTo(serverAddress, 0);
            serverAddress[3] = serverBasePath;
            clientString = "http://localhost:8080" + clientString;

            using (var wrapper = new OwinHttpListener())
            {
                wrapper.Start(wrapper.Listener, env =>
                {
                    Assert.Equal(expectedBasePath, (string)env["owin.RequestPathBase"]);
                    Assert.Equal(expectedPath, (string)env["owin.RequestPath"]);
                    Assert.Equal(expectedQuery, (string)env["owin.RequestQueryString"]);
                    return TaskHelpers.Completed();
                }, CreateAddresses(fallbackAddress, serverAddress), null);

                using (var client = new HttpClient())
                {
                    HttpResponseMessage result = await client.GetAsync(clientString);
                    Assert.Equal(HttpStatusCode.OK, result.StatusCode);
                }
            }
        }

        private static CancellationToken GetCallCancelled(IDictionary<string, object> env)
        {
            return env.Get<CancellationToken>("owin.CallCancelled");
        }

        private Task<HttpResponseMessage> SendGetRequest(OwinHttpListener listener, string address)
        {
            return SendGetRequest(listener, address, ClientCertificateOption.Automatic);
        }

        private async Task<HttpResponseMessage> SendGetRequest(OwinHttpListener listener, string address, ClientCertificateOption certOptions)
        {
            using (listener)
            {
                var handler = new WebRequestHandler();

                // Ignore server cert errors.
                handler.ServerCertificateValidationCallback = (a, b, c, d) => true;
                handler.ClientCertificateOptions = certOptions;

                var client = new HttpClient(handler);
                return await client.GetAsync(address);
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

        private static IList<IDictionary<string, object>> CreateAddresses(string[] addressParts0, string[] addressParts1)
        {
            var address0 = new Dictionary<string, object>();
            address0["scheme"] = addressParts0[0];
            address0["host"] = addressParts0[1];
            address0["port"] = addressParts0[2];
            address0["path"] = addressParts0[3];

            var address1 = new Dictionary<string, object>();
            address1["scheme"] = addressParts1[0];
            address1["host"] = addressParts1[1];
            address1["port"] = addressParts1[2];
            address1["path"] = addressParts1[3];

            IList<IDictionary<string, object>> list = new List<IDictionary<string, object>>();
            list.Add(address0);
            list.Add(address1);
            return list;
        }
    }
}
