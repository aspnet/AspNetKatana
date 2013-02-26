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
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
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
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class ClientCertificateTests : TestBase
    {
        private const HttpStatusCode CertFoundStausCode = HttpStatusCode.Accepted;
        private const HttpStatusCode CertNotFoundStausCode = HttpStatusCode.NotFound;

        public void DontAccessCertificate(IAppBuilder app)
        {
            app.Run((AppFunc)(env =>
            {
                env["owin.ResponseStatusCode"] = (int)CertNotFoundStausCode;
                return TaskHelpers.Completed();
            }));
        }

        public void CheckClientCertificate(IAppBuilder app)
        {
            app.Run((AppFunc)(
                env =>
                {
                    Func<Task> certLoader = env.Get<Func<Task>>("ssl.LoadClientCertAsync");
                    if (certLoader != null)
                    {
                        return certLoader().Then(() =>
                        {
                            X509Certificate asyncCert = env.Get<X509Certificate>("ssl.ClientCertificate");
                            env["owin.ResponseStatusCode"] = asyncCert == null ? (int)CertNotFoundStausCode : (int)CertFoundStausCode;
                        });
                    }
                    X509Certificate syncCert = env.Get<X509Certificate>("ssl.ClientCertificate");
                    env["owin.ResponseStatusCode"] = syncCert == null ? (int)CertNotFoundStausCode : (int)CertFoundStausCode;
                    return TaskHelpers.Completed();
                }));
        }

        [Theory]
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
                .Then(response => Assert.Equal(CertNotFoundStausCode, response.StatusCode))
                .Finally(() => ServicePointManager.ServerCertificateValidationCallback = null);
        }

        [Theory]
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
                .Then(response => Assert.Equal(CertNotFoundStausCode, response.StatusCode))
                .Finally(() => ServicePointManager.ServerCertificateValidationCallback = null);
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task ValidCertProvided_DontAccessCertificate_Success(string serverName)
        {
            ServicePointManager.ServerCertificateValidationCallback = AcceptAllCerts;

            int port = RunWebServer(
                serverName,
                DontAccessCertificate,
                https: true);

            HttpClientHandler handler = new HttpClientHandler();
            handler.ClientCertificateOptions = ClientCertificateOption.Automatic;
            HttpClient client = new HttpClient(handler);
            client.Timeout = TimeSpan.FromSeconds(5);
            return client.GetAsync("https://localhost:" + port)
                .Then(response => Assert.Equal(CertNotFoundStausCode, response.StatusCode))
                .Finally(() => ServicePointManager.ServerCertificateValidationCallback = null);
        }

        // IIS needs this section in applicationhost.config:
        // <system.webServer><security><access sslFlags="SslNegotiateCert" />...
        // http://www.iis.net/configreference/system.webserver/security/access
        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task ValidCertProvided_CheckClientCertificate_Success(string serverName)
        {
            ServicePointManager.ServerCertificateValidationCallback = AcceptAllCerts;

            int port = RunWebServer(
                serverName,
                CheckClientCertificate,
                https: true);

            HttpClientHandler handler = new HttpClientHandler();
            handler.ClientCertificateOptions = ClientCertificateOption.Automatic;
            HttpClient client = new HttpClient(handler);
            client.Timeout = TimeSpan.FromSeconds(5);
            return client.GetAsync("https://localhost:" + port)
                .Then(response => Assert.Equal(CertFoundStausCode, response.StatusCode))
                .Finally(() => ServicePointManager.ServerCertificateValidationCallback = null);
        }

        private bool AcceptAllCerts(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}