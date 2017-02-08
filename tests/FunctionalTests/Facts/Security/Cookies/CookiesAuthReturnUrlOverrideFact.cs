// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net;
using System.Net.Http;
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
        public void Security_ReturnUrlAndSecureCookie(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                string applicationUrl = deployer.Deploy(hostType, ReturnUrlAndSecureCookieConfiguration);
                string secureServerUri = new UriBuilder(applicationUrl) { Scheme = Uri.UriSchemeHttps }.Uri.AbsoluteUri;

                HttpClientHandler handler = new HttpClientHandler();
                HttpClient httpClient = new HttpClient(handler);

                // Unauthenticated request - verify Redirect url
                HttpResponseMessage response = httpClient.GetAsync(applicationUrl).Result;
                CookiesCommon.IsRedirectedToCookiesLogin(response.RequestMessage.RequestUri, applicationUrl, "Unauthenticated requests not automatically redirected to login page", "MyRedirectUrl");

                var validCookieCredentials = new FormUrlEncodedContent(new kvp[] { new kvp("username", "test"), new kvp("password", "test") });
                response = httpClient.PostAsync(response.RequestMessage.RequestUri, validCookieCredentials).Result;
                response.EnsureSuccessStatusCode();

                //Verify cookie sent
                Assert.False(handler.CookieContainer.Count != 1, "Forms auth cookie not received automatically after successful login");
                Cookie loginCookie = handler.CookieContainer.GetCookies(new Uri(secureServerUri))[0];

                Assert.Equal(loginCookie.Secure, true);
            }
        }

        public void ReturnUrlAndSecureCookieConfiguration(IAppBuilder app)
        {
            app.UseCookieAuthentication(new CookieAuthenticationOptions() { LoginPath = new PathString("/Auth/CookiesLogin"), ReturnUrlParameter = "MyRedirectUrl", CookieSecure = CookieSecureOption.Always });
            app.UseProtectedResource();
        }
    }
}
