// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FunctionalTests.Common;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using Xunit;
using Xunit.Extensions;
using kvp = System.Collections.Generic.KeyValuePair<string, string>;

namespace FunctionalTests.Facts.Security
{
    public partial class CookiesAuthenticationFacts
    {
        private static FormUrlEncodedContent GetValidCookieCredentials()
        {
            return new FormUrlEncodedContent(new kvp[] { new kvp("username", "test"), new kvp("password", "test") });
        }

        private static FormUrlEncodedContent GetInValidCookieCredentials()
        {
            return new FormUrlEncodedContent(new kvp[] { new kvp("username", "invaliduser"), new kvp("password", "invalidpwd") });
        }

        [Theory, Trait("FunctionalTests", "Security")]
        [InlineData(HostType.IIS)]
        [InlineData(HostType.HttpListener)]
        public void Security_CookiesAuthDefaults(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                string applicationUrl = deployer.Deploy(hostType, CookieAuthDefaultsConfiguration);
                string homePath = applicationUrl + "Auth/Home";
                string logoutPath = applicationUrl + string.Format("Auth/Logout?ReturnUrl={0}", new Uri(homePath).AbsolutePath);

                var handler = new HttpClientHandler();
                var httpClient = new HttpClient(handler);

                // Unauthenticated request
                var response = httpClient.GetAsync(applicationUrl).Result;
                CookiesCommon.IsRedirectedToCookiesLogin(response.RequestMessage.RequestUri, applicationUrl, "Unauthenticated requests not automatically redirected to login page");

                // Invalid credentials test
                for (int retryCount = 0; retryCount < 2; retryCount++)
                {
                    response = httpClient.PostAsync(response.RequestMessage.RequestUri, GetInValidCookieCredentials()).Result;
                    CookiesCommon.IsRedirectedToCookiesLogin(response.RequestMessage.RequestUri, applicationUrl, "Invalid credentials - not automatically redirecting to login page with proper ReturnUrl");
                }

                // Valid credentials
                response = httpClient.PostAsync(response.RequestMessage.RequestUri, GetValidCookieCredentials()).Result;
                Assert.Equal<string>(applicationUrl, response.RequestMessage.RequestUri.AbsoluteUri);

                //Verify cookie sent
                Assert.False(handler.CookieContainer.Count != 1 ||
                            handler.CookieContainer.GetCookies(new Uri(applicationUrl))[0].Name != "KATANACOOKIE",
                            string.Format("Forms auth cookie with expected name '{0}' not received automatically after successful login", "KATANACOOKIE"));

                //Retry multiple times with valid cookie to test sliding expiration
                for (int retryCount = 0; retryCount < 3; retryCount++)
                {
                    Thread.Sleep(2 * 1000);
                    response = httpClient.GetAsync(applicationUrl).Result;
                    response.EnsureSuccessStatusCode();
                    Assert.Equal<string>("ProtectedResource", response.Content.ReadAsStringAsync().Result);
                    Assert.Equal<string>(applicationUrl, response.RequestMessage.RequestUri.AbsoluteUri);
                }

                //Expire the cookie & verify if request is redirected to login page again
                Thread.Sleep(3 * 1000);
                response = httpClient.GetAsync(applicationUrl).Result;
                response.EnsureSuccessStatusCode();
                CookiesCommon.IsRedirectedToCookiesLogin(response.RequestMessage.RequestUri, applicationUrl, "Request not automatically redirected to login page after cookie expiry");

                //Reauthenticate with valid cookie & verify if protected resource is accessible again
                response = httpClient.PostAsync(response.RequestMessage.RequestUri, GetValidCookieCredentials()).Result;
                Assert.Equal<string>("ProtectedResource", response.Content.ReadAsStringAsync().Result);
                Assert.Equal<string>(applicationUrl, response.RequestMessage.RequestUri.AbsoluteUri);

                //Make one successful call
                response = httpClient.GetAsync(applicationUrl).Result;

                //Now corrupt the cookie to see if this gets redirected to login page
                string backUpCookieValue = handler.CookieContainer.GetCookies(new Uri(applicationUrl))[0].Value;
                handler.CookieContainer.GetCookies(new Uri(applicationUrl))[0].Value = "invalid cookie value";
                response = httpClient.GetAsync(applicationUrl).Result;

                CookiesCommon.IsRedirectedToCookiesLogin(response.RequestMessage.RequestUri, applicationUrl, "Request not automatically redirected to login page after cookie expiry");

                //put back the valid cookie & verify protected resource is accessible again
                handler.CookieContainer.GetCookies(new Uri(applicationUrl))[0].Value = backUpCookieValue;
                response = httpClient.GetAsync(applicationUrl).Result;
                Assert.Equal<string>("ProtectedResource", response.Content.ReadAsStringAsync().Result);
                Assert.Equal<string>(applicationUrl, response.RequestMessage.RequestUri.AbsoluteUri);

                //Logout the client
                response = httpClient.GetAsync(logoutPath).Result;
                Assert.True(handler.CookieContainer.Count == 0, "Cookie is not cleared on logout");
                Assert.Equal<string>("Welcome Home", response.Content.ReadAsStringAsync().Result);

                //Try accessing protected resource again. It should get redirected to login page
                response = httpClient.GetAsync(applicationUrl).Result;
                CookiesCommon.IsRedirectedToCookiesLogin(response.RequestMessage.RequestUri, applicationUrl, "Request not automatically redirected to login page after cookie expiry");
            }
        }

        public void CookieAuthDefaultsConfiguration(IAppBuilder app)
        {
            app.UseCookieAuthentication(new CookieAuthenticationOptions()
            {
                CookieName = "KATANACOOKIE",
                LoginPath = new PathString("/Auth/CookiesLogin"),
                LogoutPath = new PathString("/Auth/Logout"),
                ExpireTimeSpan = TimeSpan.FromSeconds(3),
                Provider = new CookieAuthenticationProvider()
                {
                    OnResponseSignIn = context =>
                    {
                        context.Identity.AddClaim(new System.Security.Claims.Claim("ResponseSignIn", "true"));
                    },
                    OnValidateIdentity = context =>
                    {
                        context.Identity.AddClaim(new System.Security.Claims.Claim("ValidateIdentity", "true"));
                        return Task.FromResult(0);
                    }
                }
            });

            app.UseProtectedResource();
        }
    }
}