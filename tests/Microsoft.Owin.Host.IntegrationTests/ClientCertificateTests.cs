// <copyright file="ClientCertificateTests.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Owin;
using Xunit;
using Xunit.Extensions;

#if NET40
namespace Microsoft.Owin.Host40.IntegrationTests
#else
namespace Microsoft.Owin.Host45.IntegrationTests
#endif
{
    public class ClientCertificateTests : TestBase
    {
        private const HttpStatusCode CertFound = HttpStatusCode.Accepted;
        private const HttpStatusCode CertNotFound = HttpStatusCode.NotFound;
        private const HttpStatusCode CertFoundWithErrors = HttpStatusCode.ExpectationFailed;

        public void DontAccessCertificate(IAppBuilder app)
        {
            app.Run(context =>
            {
                context.Response.StatusCode = (int)CertNotFound;
                return TaskHelpers.Completed();
            });
        }

        public void CheckClientCertificate(IAppBuilder app)
        {
            app.Run(context =>
                {
                    Func<Task> certLoader = context.Get<Func<Task>>("ssl.LoadClientCertAsync");
                    if (certLoader != null)
                    {
                        return certLoader().Then(() =>
                        {
                            X509Certificate asyncCert = context.Get<X509Certificate>("ssl.ClientCertificate");
                            Exception asyncCertError = context.Get<Exception>("ssl.ClientCertificateErrors");
                            context.Response.StatusCode = asyncCert == null ? (int)CertNotFound
                                : asyncCertError == null ? (int)CertFound : (int)CertFoundWithErrors;
                        });
                    }
                    X509Certificate syncCert = context.Get<X509Certificate>("ssl.ClientCertificate");
                    Exception syncCertError = context.Get<Exception>("ssl.ClientCertificateErrors");
                    context.Response.StatusCode = syncCert == null ? (int)CertNotFound
                        : syncCertError == null ? (int)CertFound : (int)CertFoundWithErrors;
                    return TaskHelpers.Completed();
                });
        }

        [Theory, Trait("scheme", "https")]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task NoCertProvided_DontAccessCertificate_Success(string serverName)
        {
            ServicePointManager.ServerCertificateValidationCallback = AcceptAllCerts;

            int port = RunWebServer(
                serverName,
                DontAccessCertificate,
                https: true);

            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            return client.GetAsync("https://localhost:" + port)
                .Then(response => Assert.Equal(CertNotFound, response.StatusCode))
                .Finally(() => ServicePointManager.ServerCertificateValidationCallback = null);
        }

        [Theory, Trait("scheme", "https")]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task NoCertProvided_CheckClientCertificate_Success(string serverName)
        {
            ServicePointManager.ServerCertificateValidationCallback = AcceptAllCerts;

            int port = RunWebServer(
                serverName,
                CheckClientCertificate,
                https: true);

            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            return client.GetAsync("https://localhost:" + port)
                .Then(response => Assert.Equal(CertNotFound, response.StatusCode))
                .Finally(() => ServicePointManager.ServerCertificateValidationCallback = null);
        }

        [Theory, Trait("scheme", "https")]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task ValidCertProvided_DontAccessCertificate_Success(string serverName)
        {
            ServicePointManager.ServerCertificateValidationCallback = AcceptAllCerts;

            int port = RunWebServer(
                serverName,
                DontAccessCertificate,
                https: true);

            X509Certificate2 clientCert = FindClientCert();
            Assert.NotNull(clientCert);
            WebRequestHandler handler = new WebRequestHandler();
            handler.ClientCertificates.Add(clientCert);
            HttpClient client = new HttpClient(handler);
            client.Timeout = TimeSpan.FromSeconds(5);
            return client.GetAsync("https://localhost:" + port)
                .Then(response => Assert.Equal(CertNotFound, response.StatusCode))
                .Finally(() => ServicePointManager.ServerCertificateValidationCallback = null);
        }

        // IIS needs this section in applicationhost.config:
        // <system.webServer><security><access sslFlags="SslNegotiateCert" />...
        // http://www.iis.net/configreference/system.webserver/security/access
        [Theory, Trait("scheme", "https")]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task ValidCertProvided_CheckClientCertificate_Success(string serverName)
        {
            ServicePointManager.ServerCertificateValidationCallback = AcceptAllCerts;

            int port = RunWebServer(
                serverName,
                CheckClientCertificate,
                https: true);

            X509Certificate2 clientCert = FindClientCert();
            Assert.NotNull(clientCert);
            WebRequestHandler handler = new WebRequestHandler();
            handler.ClientCertificates.Add(clientCert);
            HttpClient client = new HttpClient(handler);
            client.Timeout = TimeSpan.FromSeconds(5);
            return client.GetAsync("https://localhost:" + port)
                .Then(response => Assert.Equal(CertFound, response.StatusCode))
                .Finally(() => ServicePointManager.ServerCertificateValidationCallback = null);
        }

        [Theory, Trait("scheme", "https")]
        [InlineData("Microsoft.Owin.Host.SystemWeb", HttpStatusCode.Forbidden)]
        [InlineData("Microsoft.Owin.Host.HttpListener", CertNotFound)]
        public Task SelfSignedCertProvided_DontAccessCertificate_Success(string serverName, HttpStatusCode expectedResult)
        {
            ServicePointManager.ServerCertificateValidationCallback = AcceptAllCerts;

            int port = RunWebServer(
                serverName,
                DontAccessCertificate,
                https: true);

            WebRequestHandler handler = new WebRequestHandler();
            handler.ClientCertificates.Add(new X509Certificate2(@"SelfSignedClientCert.pfx", "katana"));
            HttpClient client = new HttpClient(handler);
            client.Timeout = TimeSpan.FromSeconds(5);
            return client.GetAsync("https://localhost:" + port)
                .Then(response => Assert.Equal(expectedResult, response.StatusCode))
                .Finally(() => ServicePointManager.ServerCertificateValidationCallback = null);
        }

        [Theory, Trait("scheme", "https")]
        [InlineData("Microsoft.Owin.Host.SystemWeb", HttpStatusCode.Forbidden)]
        [InlineData("Microsoft.Owin.Host.HttpListener", CertFoundWithErrors)]
        public Task SelfSignedCertProvided_CheckClientCertificate_Success(string serverName, HttpStatusCode expectedResult)
        {
            ServicePointManager.ServerCertificateValidationCallback = AcceptAllCerts;

            int port = RunWebServer(
                serverName,
                CheckClientCertificate,
                https: true);

            WebRequestHandler handler = new WebRequestHandler();
            handler.ClientCertificates.Add(new X509Certificate2(@"SelfSignedClientCert.pfx", "katana"));
            HttpClient client = new HttpClient(handler);
            client.Timeout = TimeSpan.FromSeconds(5);
            return client.GetAsync("https://localhost:" + port)
                .Then(response => Assert.Equal(expectedResult, response.StatusCode))
                .Finally(() => ServicePointManager.ServerCertificateValidationCallback = null);
        }

        private bool AcceptAllCerts(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        private X509Certificate2 FindClientCert()
        {
            X509Store store = new X509Store();
            store.Open(OpenFlags.ReadOnly);

            foreach (X509Certificate2 cert in store.Certificates)
            {
                bool isClientAuth = false;
                bool isSmartCard = false;
                foreach (X509Extension extension in cert.Extensions)
                {
                    X509EnhancedKeyUsageExtension eku = extension as X509EnhancedKeyUsageExtension;
                    if (eku != null)
                    {
                        foreach (Oid oid in eku.EnhancedKeyUsages)
                        {
                            if (oid.FriendlyName == "Client Authentication")
                            {
                                isClientAuth = true;
                            }
                            else if (oid.FriendlyName == "Smart Card Logon")
                            {
                                isSmartCard = true;
                                break;
                            }
                        }
                    }
                }

                if (isClientAuth && !isSmartCard && cert.Verify())
                {
                    return cert;
                }
            }
            return null;
        }
    }
}