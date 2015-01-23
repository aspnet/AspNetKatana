// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Net.Http;
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
        [Theory, Trait("FunctionalTests", "Security")]
        [InlineData(HostType.IIS)]
        [InlineData(HostType.HttpListener)]
        public void Security_PersistentCookie(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                string applicationUrl = deployer.Deploy(hostType, PersistentCookieConfiguration);
                string homePath = applicationUrl + "Auth/Home";
                string logoutPath = applicationUrl + string.Format("Auth/Logout?ReturnUrl={0}", new Uri(homePath).AbsolutePath);

                var handler = new HttpClientHandler();
                var httpClient = new HttpClient(handler);

                // Unauthenticated request
                var response = httpClient.GetAsync(applicationUrl).Result;
                CookiesCommon.IsRedirectedToCookiesLogin(response.RequestMessage.RequestUri, applicationUrl, "Unauthenticated requests not automatically redirected to login page");

                // Valid credentials
                var validPersistingCredentials = new FormUrlEncodedContent(new kvp[] { new kvp("username", "test"), new kvp("password", "test"), new kvp("rememberme", "on") });
                response = httpClient.PostAsync(response.RequestMessage.RequestUri, validPersistingCredentials).Result;
                response.EnsureSuccessStatusCode();
                Assert.Equal<string>(applicationUrl, response.RequestMessage.RequestUri.AbsoluteUri);

                //Verify cookie sent
                Assert.True(handler.CookieContainer.Count == 1, "Did not receive one cookie as expected");
                var cookie = handler.CookieContainer.GetCookies(new Uri(applicationUrl))[0];
                Assert.True(cookie != null && cookie.Name == "KATANACOOKIE", "Cookie with name 'KATANACOOKIE' not found");
                Assert.True((cookie.Expires - DateTime.Now).Days > 10, "Did not receive a persistent cookie");

                //Logout the client
                response = httpClient.GetAsync(logoutPath).Result;
                Assert.True(handler.CookieContainer.Count == 0, "Cookie is not cleared on logout");
                Assert.Equal<string>(homePath, response.RequestMessage.RequestUri.AbsoluteUri);

                //Try accessing protected resource again. It should get redirected to login page
                response = httpClient.GetAsync(applicationUrl).Result;
                CookiesCommon.IsRedirectedToCookiesLogin(response.RequestMessage.RequestUri, applicationUrl, "Request not automatically redirected to login page after cookie expiry");
            }
        }

        public void PersistentCookieConfiguration(IAppBuilder app)
        {
            app.UseCookieAuthentication(new CookieAuthenticationOptions()
            {
                CookieName = "KATANACOOKIE",
                LoginPath = new PathString("/Auth/CookiesLogin"),
                LogoutPath = new PathString("/Auth/Logout"),
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