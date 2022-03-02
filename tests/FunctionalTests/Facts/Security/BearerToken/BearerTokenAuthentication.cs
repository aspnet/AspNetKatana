// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using FunctionalTests.Common;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using Owin;
using Xunit;
using Xunit.Extensions;

namespace FunctionalTests.Facts.Security.BearerToken
{
    public class BearerTokenAuthentication
    {
        [Theory, Trait("FunctionalTests", "Security")]
        [InlineData(HostType.IIS)]
        [InlineData(HostType.HttpListener)]
        public async Task Security_BearerAuthenticationWithProvider(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                string applicationUrl = deployer.Deploy(hostType, BearerTokenAuthenticationWithProviderConfiguration);
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
                    Assert.Equal("Bearer", await response.Content.ReadAsStringAsync());
                }
            }
        }

        internal void BearerTokenAuthenticationWithProviderConfiguration(IAppBuilder app)
        {
            var bearerOptions = new OAuthBearerAuthenticationOptions()
            {
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

            app.UseOAuthBearerAuthentication(bearerOptions);

            app.Map("/BearerAuthenticationToken", subApp =>
                {
                    subApp.Run(async context =>
                        {
                            var identity = new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, "test") }, bearerOptions.AuthenticationType, ClaimTypes.Name, ClaimTypes.Role);
                            identity.AddClaim(new Claim(identity.RoleClaimType, "Guest", ClaimValueTypes.String));

                            var ticket = bool.Parse(context.Request.Query["issueExpiredToken"]) ?
                                new AuthenticationTicket(identity, new AuthenticationProperties() { ExpiresUtc = DateTime.UtcNow }) :
                                new AuthenticationTicket(identity, new AuthenticationProperties() { ExpiresUtc = DateTime.UtcNow.AddYears(4) });

                            await context.Response.WriteAsync(bearerOptions.AccessTokenFormat.Protect(ticket));
                        });
                });

            app.UseBearerApplication();
        }
    }
}