// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using FunctionalTests.Common;
using Microsoft.Owin;
using Microsoft.Owin.Security;
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
        public void Security_HttpCookieOnlyAndCookiePath(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                string applicationUrl = deployer.Deploy(hostType, HttpCookieOnlyAndCookiePathConfiguration);
                string passiveAuthLoginPage = applicationUrl + "Auth/PassiveAuthLogin";
                string homePath = applicationUrl + "Auth/Home";
                string logoutPath = applicationUrl + string.Format("Auth/Logout?ReturnUrl={0}", new Uri(homePath).AbsolutePath);

                var handler = new HttpClientHandler();
                var httpClient = new HttpClient(handler);

                var response = httpClient.GetAsync(passiveAuthLoginPage).Result;
                CookiesCommon.IsRedirectedToCookiesLogin(response.RequestMessage.RequestUri, passiveAuthLoginPage, "Unauthenticated requests not automatically redirected to login page");
                var validCookieCredentials = new FormUrlEncodedContent(new kvp[] { new kvp("username", "test"), new kvp("password", "test") });
                response = httpClient.PostAsync(response.RequestMessage.RequestUri, validCookieCredentials).Result;

                //Verify cookie sent
                Assert.Equal(2, handler.CookieContainer.Count);
                CookieCollection cookies = handler.CookieContainer.GetCookies(new Uri(applicationUrl + "Auth"));
                Cookie applicationCookie = cookies[CookieAuthenticationDefaults.CookiePrefix + "Application"];
                Cookie temporaryCookie = cookies["TemporaryCookie"];
                Assert.NotNull(applicationCookie);
                Assert.NotNull(temporaryCookie);

                Assert.True(applicationCookie.HttpOnly);
                Assert.Equal("/", applicationCookie.Path);
                Assert.False(temporaryCookie.HttpOnly);
                Assert.Equal(temporaryCookie.Path, new Uri(applicationUrl).AbsolutePath + "Auth");
                Assert.Equal(applicationUrl, response.RequestMessage.RequestUri.AbsoluteUri);

                //Logout the client
                response = httpClient.GetAsync(logoutPath).Result;
                Assert.True(handler.CookieContainer.Count == 0, "Cookie is not cleared on logout");
                Assert.Equal(homePath, response.RequestMessage.RequestUri.AbsoluteUri);
            }
        }

        internal void HttpCookieOnlyAndCookiePathConfiguration(IAppBuilder app)
        {
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "Application",
                CookieName = ".AspNet.Application",
                LoginPath = new PathString("/Account/LogOn"),
                LogoutPath = new PathString("/Account/Logout")
            });

            string cookiePath = HostingEnvironment.IsHosted ? HttpRuntime.AppDomainAppVirtualPath + "/Auth" : "/Auth";

            app.UseCookieAuthentication(
                new CookieAuthenticationOptions()
                {
                    LoginPath = new PathString("/Auth/CookiesLogin"),
                    LogoutPath = new PathString("/Auth/Logout"),
                    CookieName = "TemporaryCookie",
                    AuthenticationMode = AuthenticationMode.Passive,
                    CookieHttpOnly = false,
                    CookiePath = cookiePath,
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
