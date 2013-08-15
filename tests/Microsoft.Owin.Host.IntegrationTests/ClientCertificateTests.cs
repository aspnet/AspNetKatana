// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Net;
using System.Net.Http;
using System.Net.Security;
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
                var certLoader = context.Get<Func<Task>>("ssl.LoadClientCertAsync");
                if (certLoader != null)
                {
                    return certLoader().Then(() =>
                    {
                        var asyncCert = context.Get<X509Certificate>("ssl.ClientCertificate");
                        var asyncCertError = context.Get<Exception>("ssl.ClientCertificateErrors");
                        context.Response.StatusCode = asyncCert == null ? (int)CertNotFound
                            : asyncCertError == null ? (int)CertFound : (int)CertFoundWithErrors;
                    });
                }
                var syncCert = context.Get<X509Certificate>("ssl.ClientCertificate");
                var syncCertError = context.Get<Exception>("ssl.ClientCertificateErrors");
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

            var client = new HttpClient();
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

            var client = new HttpClient();
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
            var handler = new WebRequestHandler();
            handler.ClientCertificates.Add(clientCert);
            var client = new HttpClient(handler);
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
            var handler = new WebRequestHandler();
            handler.ClientCertificates.Add(clientCert);
            var client = new HttpClient(handler);
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

            var handler = new WebRequestHandler();
            handler.ClientCertificates.Add(new X509Certificate2(@"SelfSignedClientCert.pfx", "katana"));
            var client = new HttpClient(handler);
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

            var handler = new WebRequestHandler();
            handler.ClientCertificates.Add(new X509Certificate2(@"SelfSignedClientCert.pfx", "katana"));
            var client = new HttpClient(handler);
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
            var store = new X509Store();
            store.Open(OpenFlags.ReadOnly);

            foreach (var cert in store.Certificates)
            {
                bool isClientAuth = false;
                bool isSmartCard = false;
                foreach (var extension in cert.Extensions)
                {
                    var eku = extension as X509EnhancedKeyUsageExtension;
                    if (eku != null)
                    {
                        foreach (var oid in eku.EnhancedKeyUsages)
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
