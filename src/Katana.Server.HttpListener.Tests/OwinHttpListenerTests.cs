//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Katana.Server.HttpListener.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;
    using Katana.Server.HttpListenerWrapper;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Owin;

    // TODO: Convert to XUnit?
    // These tests measure that the core HttpListener wrapper functions as expected in normal and exceptional scenarios.
    // NOTE: These tests require SetupProject.bat to be run as admin from a VS command prompt once per machine.
    [TestClass]
    public class OwinHttpListenerTests
    {
        private const string HttpServerAddress = "http://*:8080/BaseAddress/";
        private const string HttpClientAddress = "http://localhost:8080/BaseAddress/";
        private const string HttpsServerAddress = "https://*:9090/BaseAddress/";
        private const string HttpsClientAddress = "https://localhost:9090/BaseAddress/";

        private AppDelegate notImplemented = call => { throw new NotImplementedException(); };

        [TestMethod]
        public void OwinHttpListener_CreatedStartedStoppedDisposed_Success()
        {
            OwinHttpListener listener = new OwinHttpListener(this.notImplemented, HttpServerAddress);
            using (listener)
            {
                listener.Start();
                listener.Stop();
                listener.Dispose();
            }
        }

        // TODO: HTTPS requires pre-configing the server cert to work
        [TestMethod]
        public void OwinHttpListener_HttpsCreatedStartedStoppedDisposed_Success()
        {
            OwinHttpListener listener = new OwinHttpListener(this.notImplemented, HttpsServerAddress);
            using (listener)
            {
                listener.Start();
                listener.Stop();
                listener.Dispose();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_NullDelegate_Throws()
        {
            OwinHttpListener listener = new OwinHttpListener(null, HttpServerAddress);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Ctor_BadServerAddress_Throws()
        {
            OwinHttpListener listener = new OwinHttpListener(this.notImplemented, "http://host:9090/BadPathDoesntEndInSlash");
        }

        [TestMethod]
        public async Task EndToEnd_GetRequest_Success()
        {
            OwinHttpListener listener = new OwinHttpListener(call => this.CreateEmptyResponseTask(200), HttpServerAddress);
            HttpResponseMessage response = await this.SendGetRequest(listener, HttpClientAddress);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(0, response.Content.Headers.ContentLength.Value);
        }

        [TestMethod]
        public async Task EndToEnd_SingleThreadedTwoGetRequests_Success()
        {
            OwinHttpListener listener = new OwinHttpListener(call => this.CreateEmptyResponseTask(200), HttpServerAddress);
            using (listener)
            {
                listener.Start(1);
                HttpClient client = new HttpClient();
                string result = await client.GetStringAsync(HttpClientAddress);
                Assert.AreEqual(string.Empty, result);
                result = await client.GetStringAsync(HttpClientAddress);
                Assert.AreEqual(string.Empty, result);
            }
        }

        [TestMethod]
        public async Task EndToEnd_GetRequestWithDispose_Success()
        {
            bool disposeCalled = false;

            OwinHttpListener listener = new OwinHttpListener(
                call => 
                {
                    call.Completed.Register(() => disposeCalled = true);
                    return this.CreateEmptyResponseTask(200);
                }, 
                HttpServerAddress);

            HttpResponseMessage response = await this.SendGetRequest(listener, HttpClientAddress);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(0, response.Content.Headers.ContentLength.Value);
            Assert.IsTrue(disposeCalled);
        }

        [TestMethod]
        public async Task EndToEnd_HttpsGetRequest_Success()
        {
            OwinHttpListener listener = new OwinHttpListener(
                call => 
                {
                    object obj;
                    Assert.IsTrue(call.Environment.TryGetValue("owin.ClientCertificate", out obj));
                    Assert.IsNotNull(obj);
                    Assert.IsInstanceOfType(obj, typeof(X509Certificate2));
                    return CreateEmptyResponseTask(200);
                }, 
                HttpsServerAddress);

            HttpResponseMessage response = await this.SendGetRequest(listener, HttpsClientAddress, ClientCertificateOption.Automatic);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(0, response.Content.Headers.ContentLength.Value);
        }

        [TestMethod]
        public async Task EndToEnd_HttpsGetRequestNoClientCert_Success()
        {
            OwinHttpListener listener = new OwinHttpListener(
                call =>
                {
                    object obj;
                    Assert.IsFalse(call.Environment.TryGetValue("owin.ClientCertificate", out obj));
                    return this.CreateEmptyResponseTask(200);
                }, 
                HttpsServerAddress);

            HttpResponseMessage response = await this.SendGetRequest(listener, HttpsClientAddress, ClientCertificateOption.Manual);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(0, response.Content.Headers.ContentLength.Value);
        }

        [TestMethod]
        public async Task AppDelegate_ThrowsSync_500Error()
        {
            OwinHttpListener listener = new OwinHttpListener(this.notImplemented, HttpServerAddress);
            HttpResponseMessage response = await this.SendGetRequest(listener, HttpClientAddress);
            Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.AreEqual(0, response.Content.Headers.ContentLength.Value);
        }

        [TestMethod]
        public async Task AppDelegate_ReturnsExceptionAsync_500Error()
        {
            bool disposeCalled = false;

            OwinHttpListener listener = new OwinHttpListener(
                async call =>
                {
                    call.Completed.Register(() => disposeCalled = true);
                    await Task.Delay(1);
                    throw new NotImplementedException();
                },
                HttpServerAddress);

            HttpResponseMessage response = await this.SendGetRequest(listener, HttpClientAddress);
            Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.AreEqual(0, response.Content.Headers.ContentLength.Value);
            Assert.IsTrue(disposeCalled);
        }

        [TestMethod]
        public async Task BodyDelegate_PostEchoRequest_Success()
        {
            bool disposeCalled = false;

            OwinHttpListener listener = new OwinHttpListener(
                call =>
                {
                    call.Completed.Register(() => disposeCalled = true);
                    ResultParameters results = CreateEmptyResponse(200);
                    results.Headers.Add("Content-Length", call.Headers["Content-Length"]);

                    results.Body = (stream, cancel) =>
                    {
                        Assert.IsFalse(disposeCalled);
                        Assert.IsNotNull(stream);
                        return call.Body.CopyToAsync(stream, 1024, cancel);
                    };

                    return Task.FromResult(results);
                }, 
                HttpServerAddress);

            using (listener)
            {
                listener.Start(1);
                HttpClient client = new HttpClient();
                string dataString = "Hello World";
                HttpResponseMessage result = await client.PostAsync(HttpClientAddress, new StringContent(dataString));
                result.EnsureSuccessStatusCode();
                Assert.AreEqual(dataString.Length, result.Content.Headers.ContentLength.Value);
                Assert.AreEqual(dataString, await result.Content.ReadAsStringAsync());
                Thread.Sleep(100); // Wait for the server to finish cleanup on its side.
                Assert.IsTrue(disposeCalled);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpRequestException))]
        public async Task BodyDelegate_ThrowsSync_ConnectionClosed()
        {
            bool disposeCalled = false;
            OwinHttpListener listener = new OwinHttpListener(
                call =>
                {
                    call.Completed.Register(() => disposeCalled = true);
                    ResultParameters response = CreateEmptyResponse(200);
                    response.Headers.Add("Content-Length", new string[] { "10" });

                    response.Body = (stream, cancel) =>
                    {
                        throw new NotImplementedException();
                    };

                    return Task.FromResult(response);
                },
                HttpServerAddress);

            try
            {
                await this.SendGetRequest(listener, HttpClientAddress);
            }
            finally
            {
                Assert.IsTrue(disposeCalled);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpRequestException))]
        public async Task BodyDelegate_ThrowsAsync_ConnectionClosed()
        {
            bool disposeCalled = false;

            OwinHttpListener listener = new OwinHttpListener(
                call =>
                {
                    call.Completed.Register(() => disposeCalled = true);
                    ResultParameters response = CreateEmptyResponse(200);
                    response.Headers.Add("Content-Length", new string[] { "10" });

                    response.Body = async (stream, cancel) =>
                    {
                        await Task.Delay(1);
                        throw new NotImplementedException();
                    };

                    return Task.FromResult(response);
                },
                HttpServerAddress);
            
            try
            {
                await this.SendGetRequest(listener, HttpClientAddress);
            }
            finally
            {
                Assert.IsTrue(disposeCalled);
            }
        }

        [TestMethod]
        public void TimeoutArgs_Default_Infinite()
        {
            OwinHttpListener listener = new OwinHttpListener(this.notImplemented, HttpServerAddress);
            Assert.AreEqual(Timeout.InfiniteTimeSpan, listener.MaxRequestLifetime);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TimeoutArgs_Negative_Throws()
        {
            OwinHttpListener listener = new OwinHttpListener(this.notImplemented, HttpServerAddress);
            listener.MaxRequestLifetime = TimeSpan.FromSeconds(-1);
        }

        [TestMethod]
        public void TimeoutArgs_Infiniate_Success()
        {
            OwinHttpListener listener = new OwinHttpListener(this.notImplemented, HttpServerAddress);
            listener.MaxRequestLifetime = Timeout.InfiniteTimeSpan;
            Assert.AreEqual(Timeout.InfiniteTimeSpan, listener.MaxRequestLifetime);
        }

        [TestMethod]
        public void TimeoutArgs_Huge_Success()
        {
            OwinHttpListener listener = new OwinHttpListener(this.notImplemented, HttpServerAddress);
            listener.MaxRequestLifetime = TimeSpan.FromSeconds(int.MaxValue);
            Assert.AreEqual(int.MaxValue, listener.MaxRequestLifetime.TotalSeconds);
        }

        [TestMethod]
        public async Task Timeout_GetRequestWithinTimeout_Success()
        {
            OwinHttpListener listener = new OwinHttpListener(call => this.CreateEmptyResponseTask(200), HttpServerAddress);
            listener.MaxRequestLifetime = TimeSpan.FromSeconds(1);

            HttpResponseMessage response = await this.SendGetRequest(listener, HttpClientAddress);
            response.EnsureSuccessStatusCode();
        }

        [TestMethod]
        public async Task Timeout_GetRequestTimeoutDurringRequest_500Error()
        {
            OwinHttpListener listener = new OwinHttpListener(
                async call =>
                {
                    ResultParameters response = CreateEmptyResponse(200);

                    await Task.Delay(100);

                    return response;
                },
                HttpServerAddress);
            listener.MaxRequestLifetime = TimeSpan.FromMilliseconds(1);

            HttpResponseMessage result = await this.SendGetRequest(listener, HttpClientAddress);            
            Assert.AreEqual(HttpStatusCode.InternalServerError, result.StatusCode);
            Assert.AreEqual(0, result.Content.Headers.ContentLength.Value);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpRequestException))]
        public async Task Timeout_GetRequestTimeoutDurringResponse_ConnectionClose()
        {
            OwinHttpListener listener = new OwinHttpListener(
                call =>
                {
                    ResultParameters response = CreateEmptyResponse(200);
                    response.Headers.Add("Content-Length", new string[] { "10" });

                    response.Body = async (stream, cancel) =>
                    {
                        await Task.Delay(1000);
                        await stream.WriteAsync(new byte[10], 0, 10, cancel);
                    };

                    return Task.FromResult(response);
                },
                HttpServerAddress);

            listener.MaxRequestLifetime = TimeSpan.FromMilliseconds(1);
            await this.SendGetRequest(listener, HttpClientAddress);
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

        private Task<HttpResponseMessage> SendGetRequest(OwinHttpListener listener, string address)
        {
            return this.SendGetRequest(listener, address, ClientCertificateOption.Automatic);
        }

        private async Task<HttpResponseMessage> SendGetRequest(OwinHttpListener listener, string address, ClientCertificateOption certOptions)
        {
            using (listener)
            {
                listener.Start(1);

                WebRequestHandler handler = new WebRequestHandler();

                // Ignore server cert errors.
                handler.ServerCertificateValidationCallback = (a, b, c, d) => true;
                handler.ClientCertificateOptions = certOptions;

                HttpClient client = new HttpClient(handler);
                return await client.GetAsync(address);
            }
        }
    }
}
