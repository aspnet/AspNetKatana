// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
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
        public void Security_CustomSecureDataHandler(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                string applicationUrl = deployer.Deploy(hostType, CustomSecureDataHandlerConfiguration);
                string homePath = applicationUrl + "Auth/Home";
                string logoutPath = applicationUrl + string.Format("Auth/Logout?ReturnUrl={0}", new Uri(homePath).AbsolutePath);

                var handler = new HttpClientHandler();
                var httpClient = new HttpClient(handler);

                // Unauthenticated request
                var response = httpClient.GetAsync(applicationUrl).Result;
                CookiesCommon.IsRedirectedToCookiesLogin(response.RequestMessage.RequestUri, applicationUrl, "Unauthenticated requests not automatically redirected to login page");

                // Valid credentials
                var validCookieCredentials = new FormUrlEncodedContent(new kvp[] { new kvp("username", "test"), new kvp("password", "test") });
                response = httpClient.PostAsync(response.RequestMessage.RequestUri, validCookieCredentials).Result;
                Assert.Equal<string>("OnResponseSignedIn.CustomSecureDataHandler", response.Content.ReadAsStringAsync().Result);
                Assert.Equal<string>(applicationUrl, response.RequestMessage.RequestUri.AbsoluteUri);
                var cookieBackup = handler.CookieContainer.GetCookies(new Uri(applicationUrl))[".AspNet.Cookies"];

                for (int retryCount = 0; retryCount < 10; retryCount++)
                {
                    response = httpClient.GetAsync(applicationUrl).Result;
                    Assert.Equal<string>("CustomSecureDataHandler", response.Content.ReadAsStringAsync().Result);
                    Assert.Equal<string>(applicationUrl, response.RequestMessage.RequestUri.AbsoluteUri);
                }

                //Logout the client
                response = httpClient.GetAsync(logoutPath).Result;
                Assert.True(handler.CookieContainer.Count == 0, "Cookie is not cleared on logout");
                Assert.Equal<string>("Welcome Home", response.Content.ReadAsStringAsync().Result);

                //Try login with a corrupt cookie to see that Exception notification is triggered. 
                handler.CookieContainer.Add(new Cookie("ExceptionTrigger", "true", "/", cookieBackup.Domain));
                handler.CookieContainer.Add(cookieBackup);
                response = httpClient.GetAsync(applicationUrl).Result;
                Assert.Equal<string>("OnException.CustomSecureDataHandler", response.Content.ReadAsStringAsync().Result);
            }
        }

        public void CustomSecureDataHandlerConfiguration(IAppBuilder app)
        {
            //Override home action to verify something specific.
            app.Map("/Auth/Home", home =>
            {
                home.Run(async context =>
                {
                    if (context.Request.Cookies["OnResponseSignOut"] == "true")
                    {
                        context.Response.Cookies.Delete("OnResponseSignOut", new CookieOptions());
                        await context.Response.WriteAsync("Welcome Home");
                    }
                });
            });

            app.UseCookieAuthentication(new CookieAuthenticationOptions()
            {
                LoginPath = new PathString("/Auth/CookiesLogin"),
                LogoutPath = new PathString("/Auth/Logout"),
                TicketDataFormat = new CustomSecureDataFormat(),
                Provider = new CookieAuthenticationProvider()
                {
                    OnValidateIdentity = context =>
                        {
                            if (context.Request.Cookies["ExceptionTrigger"] == "true")
                            {
                                throw new Exception("OnException");
                            }

                            return Task.FromResult(0);
                        },
                    OnException = context =>
                        {
                            context.Rethrow = false;
                            context.Response.WriteAsync("OnException.");
                        },
                    OnResponseSignedIn = context =>
                        {
                            context.Response.Cookies.Append("OnResponseSignedIn", "true");
                        },
                    OnResponseSignOut = context =>
                        {
                            context.Response.Cookies.Append("OnResponseSignOut", "true");
                        }
                }
            });

            app.UseCookiesLoginSetup();
            app.Run(async context =>
                {
                    if (context.Authentication.User == null || !context.Authentication.User.Identity.IsAuthenticated)
                    {
                        context.Authentication.Challenge("Cookies");
                        await context.Response.WriteAsync("Unauthorized");
                    }
                    else
                    {
                        var identity = (context.Request.User as ClaimsPrincipal).Identity as ClaimsIdentity;

                        if (!identity.HasClaim("Protect", "true"))
                        {
                            throw new Exception("Failed");
                        }

                        if (context.Request.Cookies["OnResponseSignedIn"] == "true")
                        {
                            context.Response.Cookies.Delete("OnResponseSignedIn", new CookieOptions());
                            await context.Response.WriteAsync("OnResponseSignedIn.");
                        }

                        await context.Response.WriteAsync("CustomSecureDataHandler");
                    }
                });
        }
    }

    public class CustomSecureDataFormat : ISecureDataFormat<AuthenticationTicket>
    {
        private static AuthenticationTicket savedTicket;

        public string Protect(AuthenticationTicket data)
        {
            savedTicket = data;
            savedTicket.Identity.AddClaim(new Claim("Protect", "true"));
            return "ProtectedString";
        }

        public AuthenticationTicket Unprotect(string protectedText)
        {
            return protectedText == "ProtectedString" ? savedTicket : null;
        }
    }
}