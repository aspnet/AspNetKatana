// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using FunctionalTests.Common;
using FunctionalTests.Facts.Security.Common;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Jwt;
using Microsoft.Owin.Security.OAuth;
using Owin;
using Xunit;
using Xunit.Extensions;

namespace FunctionalTests.Facts.Security.BearerToken
{
    public class SymmetricJwtTokenAuthentication
    {
        [Theory, Trait("FunctionalTests", "Security")]
        [InlineData(HostType.HttpListener)]
        public async Task Security_SymmetricJwtTokenAuthenticationWithProvider(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                string applicationUrl = deployer.Deploy(hostType, SymmetricJwtTokenAuthenticationWithProviderConfiguration);
                string bearerAuthenticateResource = applicationUrl + "BearerAuthenticationToken?issueExpiredToken={0}";

                var httpClient = new HttpClient();

                // Unauthenticated request - must throw 401 with challenge
                var response = await httpClient.GetAsync(applicationUrl);
                Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
                Assert.Contains("bearer", response.Headers.WwwAuthenticate.ToString().ToLower());

                //Get an expired token to see if this is rejected by middleware. 
                response = await httpClient.GetAsync(string.Format(bearerAuthenticateResource, "true"));

                var secretInBody = await response.Content.ReadAsStringAsync();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", secretInBody);

                response = await httpClient.GetAsync(applicationUrl);
                Assert.NotEqual<string>("Bearer", await response.Content.ReadAsStringAsync());

                //Get a valid token to see if it works fine
                response = await httpClient.GetAsync(string.Format(bearerAuthenticateResource, "false"));

                secretInBody = await response.Content.ReadAsStringAsync();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", secretInBody);

                for (int count = 0; count < 5; count++)
                {
                    response = await httpClient.GetAsync(applicationUrl);
                    Assert.Equal<string>("Bearer", await response.Content.ReadAsStringAsync());
                }
            }
        }

        public void SymmetricJwtTokenAuthenticationWithProviderConfiguration(IAppBuilder app)
        {
            string issuer = "http://katanatesting.com/";
            var signingAlgorithm = new AesManaged();

            var SymmetricJwtOptions = new JwtBearerAuthenticationOptions()
            {
                AllowedAudiences = new string[] { issuer },
                IssuerSecurityKeyProviders = new IIssuerSecurityKeyProvider[] { new SymmetricKeyIssuerSecurityKeyProvider(issuer, signingAlgorithm.Key) },
                Provider = new OAuthBearerAuthenticationProvider()
                {
                    OnRequestToken = context =>
                    {
                        context.OwinContext.Set<bool>("OnRequestToken", true);
                        return Task.FromResult(0);
                    },
                    OnValidateIdentity = context =>
                    {
                        context.OwinContext.Set<bool>("OnValidateIdentity", true);
                        return Task.FromResult(0);
                    }
                }
            };

            //This test is to demonstrate the use of this extension method
            app.UseJwtBearerAuthentication(SymmetricJwtOptions);

            app.Map("/BearerAuthenticationToken", subApp =>
            {
                subApp.Run(async context =>
                {
                    var identity = new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, "test") }, SymmetricJwtOptions.AuthenticationType, ClaimTypes.Name, ClaimTypes.Role);
                    identity.AddClaim(new Claim(identity.RoleClaimType, "Guest", ClaimValueTypes.String));

                    var ticket = bool.Parse(context.Request.Query["issueExpiredToken"]) ?
                        new AuthenticationTicket(identity, new AuthenticationProperties() { ExpiresUtc = DateTime.UtcNow }) :
                        new AuthenticationTicket(identity, new AuthenticationProperties() { ExpiresUtc = DateTime.UtcNow.AddYears(4) });

                    var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(signingAlgorithm.Key), SecurityAlgorithms.HmacSha256Signature, SecurityAlgorithms.Sha256Digest);
                    await context.Response.WriteAsync(SecurityUtils.CreateJwtToken(ticket, issuer, signingCredentials));
                });
            });

            app.UseBearerApplication();
        }
    }
}