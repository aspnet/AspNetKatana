// <copyright file="OwinHttpListenerTests.cs" company="Katana contributors">
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
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Owin.Host.HttpListener.Tests
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    // These tests measure that the core HttpListener wrapper functions as expected in normal and exceptional scenarios.
    // NOTE: These tests require SetupProject.bat to be run as admin from a VS command prompt once per machine.
    public class OwinHttpListenerTests
    {
        private static readonly string[] HttpServerAddress = new string[] { "http://+:8080/BaseAddress/" };
        private const string HttpClientAddress = "http://localhost:8080/BaseAddress/";
        private static readonly string[] HttpsServerAddress = new string[] { "https://+:9090/BaseAddress/" };
        private const string HttpsClientAddress = "https://localhost:9090/BaseAddress/";

        private readonly AppFunc _notImplemented = env => { throw new NotImplementedException(); };

        [Fact]
        public void OwinHttpListener_CreatedStartedStoppedDisposed_Success()
        {
            var listener = new OwinHttpListener(_notImplemented, HttpServerAddress, null);
            using (listener)
            {
                listener.Start();
                listener.Stop();
            }
        }

        // HTTPS requires pre-configuring the server cert to work
        [Fact]
        public void OwinHttpListener_HttpsCreatedStartedStoppedDisposed_Success()
        {
            var listener = new OwinHttpListener(_notImplemented, HttpsServerAddress, null);
            using (listener)
            {
                listener.Start();
                listener.Stop();
            }
        }

        [Fact]
        public void Ctor_NullDelegate_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new OwinHttpListener(null, HttpServerAddress, null));
        }

        [Fact]
        public void Ctor_BadServerAddress_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
                new OwinHttpListener(_notImplemented, new string[]
                {
                    "http://host:9090/BadPathDoesntEndInSlash"
                }, null));
        }

        [Fact]
        public async Task EndToEnd_GetRequest_Success()
        {
            var listener = new OwinHttpListener(env => TaskHelpers.Completed(), HttpServerAddress, null);
            HttpResponseMessage response = await SendGetRequest(listener, HttpClientAddress);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(0, response.Content.Headers.ContentLength.Value);
        }

        [Fact]
        public async Task EndToEnd_SingleThreadedTwoGetRequests_Success()
        {
            var listener = new OwinHttpListener(env => TaskHelpers.Completed(), HttpServerAddress, null);
            using (listener)
            {
                listener.Start();
                var client = new HttpClient();
                string result = await client.GetStringAsync(HttpClientAddress);
                Assert.Equal(string.Empty, result);
                result = await client.GetStringAsync(HttpClientAddress);
                Assert.Equal(string.Empty, result);
            }
        }

        [Fact]
        public async Task EndToEnd_GetRequestWithDispose_Success()
        {
            ManualResetEvent cancelled = new ManualResetEvent(false);

            var listener = new OwinHttpListener(
                env =>
                {
                    GetCallCancelled(env).Register(() => cancelled.Set());
                    return TaskHelpers.Completed();
                },
                HttpServerAddress, null);

            HttpResponseMessage response = await SendGetRequest(listener, HttpClientAddress);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(0, response.Content.Headers.ContentLength.Value);
            Assert.False(cancelled.WaitOne(100));
        }

        [Fact]
        public async Task EndToEnd_HttpsGetRequest_Success()
        {
            var listener = new OwinHttpListener(
                async env =>
                {
                    object obj;
                    Assert.False(env.TryGetValue("ssl.ClientCertificate", out obj));
                    Assert.True(env.TryGetValue("ssl.LoadClientCertAsync", out obj));
                    Assert.NotNull(obj);
                    Assert.IsType(typeof(Func<Task>), obj);
                    Func<Task> loadCert = (Func<Task>)obj;
                    await loadCert();
                    Assert.True(env.TryGetValue("ssl.ClientCertificate", out obj));
                    Assert.NotNull(obj);
                    Assert.IsType<X509Certificate2>(obj);
                },
                HttpsServerAddress, null);

            HttpResponseMessage response = await SendGetRequest(listener, HttpsClientAddress, ClientCertificateOption.Automatic);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(0, response.Content.Headers.ContentLength.Value);
        }

        [Fact]
        public async Task EndToEnd_HttpsGetRequestNoClientCert_Success()
        {
            var listener = new OwinHttpListener(
                env =>
                {
                    object obj;
                    Assert.False(env.TryGetValue("owin.ClientCertificate", out obj));
                    return TaskHelpers.Completed();
                },
                HttpsServerAddress, null);

            HttpResponseMessage response = await SendGetRequest(listener, HttpsClientAddress, ClientCertificateOption.Manual);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(0, response.Content.Headers.ContentLength.Value);
        }

        [Fact]
        public async Task AppDelegate_ThrowsSync_500Error()
        {
            var listener = new OwinHttpListener(_notImplemented, HttpServerAddress, null);
            HttpResponseMessage response = await SendGetRequest(listener, HttpClientAddress);
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Equal(0, response.Content.Headers.ContentLength.Value);
        }

        [Fact]
        public async Task AppDelegate_ReturnsExceptionAsync_500Error()
        {
            bool callCancelled = false;

            var listener = new OwinHttpListener(
                async env =>
                {
                    GetCallCancelled(env).Register(() => callCancelled = true);
                    await Task.Delay(1);
                    throw new NotImplementedException();
                },
                HttpServerAddress, null);

            HttpResponseMessage response = await SendGetRequest(listener, HttpClientAddress);
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Equal(0, response.Content.Headers.ContentLength.Value);
            Assert.True(callCancelled);
        }

        [Fact]
        public async Task Body_PostEchoRequest_Success()
        {
            bool callCancelled = false;

            var listener = new OwinHttpListener(
                env =>
                {
                    GetCallCancelled(env).Register(() => callCancelled = true);
                    var requestHeaders = env.Get<IDictionary<string, string[]>>("owin.RequestHeaders");
                    var responseHeaders = env.Get<IDictionary<string, string[]>>("owin.ResponseHeaders");
                    responseHeaders.Add("Content-Length", requestHeaders["Content-Length"]);

                    var requestStream = env.Get<Stream>("owin.RequestBody");
                    var responseStream = env.Get<Stream>("owin.ResponseBody");

                    return requestStream.CopyToAsync(responseStream, 1024);
                },
                HttpServerAddress, null);

            using (listener)
            {
                listener.Start();
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
            var listener = new OwinHttpListener(
                env =>
                {
                    GetCallCancelled(env).Register(() => callCancelled = true);
                    var responseHeaders = env.Get<IDictionary<string, string[]>>("owin.ResponseHeaders");
                    responseHeaders.Add("Content-Length", new string[] { "10" });

                    var responseStream = env.Get<Stream>("owin.ResponseBody");
                    responseStream.WriteByte(0xFF);

                    throw new NotImplementedException();
                },
                HttpServerAddress, null);

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

            var listener = new OwinHttpListener(
                env =>
                {
                    GetCallCancelled(env).Register(() => callCancelled = true);
                    var responseHeaders = env.Get<IDictionary<string, string[]>>("owin.ResponseHeaders");
                    responseHeaders.Add("Content-Length", new string[] { "10" });

                    var responseStream = env.Get<Stream>("owin.ResponseBody");
                    responseStream.WriteByte(0xFF);

                    return TaskHelpers.FromError(new NotImplementedException());
                },
                HttpServerAddress, null);

            try
            {
                Assert.Throws<AggregateException>(() => SendGetRequest(listener, HttpClientAddress).Result);
            }
            finally
            {
                Assert.True(callCancelled);
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
                listener.Start();

                var handler = new WebRequestHandler();

                // Ignore server cert errors.
                handler.ServerCertificateValidationCallback = (a, b, c, d) => true;
                handler.ClientCertificateOptions = certOptions;

                var client = new HttpClient(handler);
                return await client.GetAsync(address);
            }
        }
    }
}
