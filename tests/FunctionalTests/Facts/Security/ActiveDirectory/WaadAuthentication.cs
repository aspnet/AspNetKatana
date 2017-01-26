// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using FunctionalTests.Common;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.ActiveDirectory;
using Owin;
using Xunit;
using Xunit.Extensions;

namespace FunctionalTests.Facts.Security.ActiveDirectory
{
    public class WaadAuthentication
    {
        [Theory, Trait("FunctionalTests", "Security")]
        [InlineData(HostType.IIS)]
        [InlineData(HostType.HttpListener)]
        public async Task Security_WaadAuthenticationWithProvider(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                string applicationUrl = deployer.Deploy(hostType, WaadAuthenticationWithProviderConfiguration);

                var httpClient = new HttpClient();

                // Unauthenticated request - must throw 401 with challenge
                var response = await httpClient.GetAsync(applicationUrl);
                Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
                Assert.Contains("bearer", response.Headers.WwwAuthenticate.ToString().ToLower());

                var secretInBody = "DummyJwtToken";
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", secretInBody);

                response = await httpClient.GetAsync(applicationUrl);
                Assert.Equal<HttpStatusCode>(HttpStatusCode.Unauthorized, response.StatusCode);
            }
        }

        public void WaadAuthenticationWithProviderConfiguration(IAppBuilder app)
        {
            app.UseWindowsAzureActiveDirectoryBearerAuthentication(new WindowsAzureActiveDirectoryBearerAuthenticationOptions
                {
                    TokenValidationParameters = new TokenValidationParameters() { ValidAudience = "http://localhost/redirectUri" },
                    Tenant = "4afbc689-805b-48cf-a24c-d4aa3248a248",
                    BackchannelCertificateValidator = new WaadCertificateValidator(),
                    BackchannelHttpHandler = new WaadChannelHttpHandler(),
                });

            app.Run(async context =>
            {
                if (context.Authentication.User == null || !context.Authentication.User.Identity.IsAuthenticated)
                {
                    context.Authentication.Challenge("Bearer");
                    await context.Response.WriteAsync("Unauthorized");
                }
                else
                {
                    if (!context.Get<bool>("OnRequestToken") || !context.Get<bool>("OnValidateIdentity"))
                    {
                        await context.Response.WriteAsync("Provider not invoked");
                    }
                    else
                    {
                        await context.Response.WriteAsync("Bearer");
                    }
                }
            });
        }
    }

    public class WaadChannelHttpHandler : WebRequestHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Add("InvalidCert", "HeaderFound");
            return await base.SendAsync(request, cancellationToken);
        }
    }

    public class WaadCertificateValidator : ICertificateValidator
    {
        public bool Validate(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            var requestHeaders = ((HttpWebRequest)sender).Headers;
            var headerFound = (requestHeaders["InvalidCert"] != null);
            return headerFound;
        }
    }
}