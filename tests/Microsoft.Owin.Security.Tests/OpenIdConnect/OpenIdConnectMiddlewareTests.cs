// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Microsoft.Owin.Testing;
using Owin;
using Xunit;
using Xunit.Extensions;

namespace Microsoft.Owin.Security.Tests.OpenIdConnect
{
    public class OpenIdConnectMiddlewareTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ChallengeIncludesPkceIfRequested(bool include)
        {
            var options = new OpenIdConnectAuthenticationOptions()
                          {
                              Authority = "https://authserver/",
                              ClientId = "Test Client Id",
                              ClientSecret = "Test Client Secret",
                              UsePkce = include,
                              ResponseType = OpenIdConnectResponseType.Code,
                              Configuration = new OpenIdConnectConfiguration()
                              {
                                  AuthorizationEndpoint = "https://authserver/auth"
                              }
            };
            var server = CreateServer(
                app => app.UseOpenIdConnectAuthentication(options),
                context =>
                {
                    context.Authentication.Challenge("OpenIdConnect");
                    return true;
                });

            var transaction = await SendAsync(server, "http://example.com/challenge");

            var res = transaction.Response;
            Assert.Equal(HttpStatusCode.Redirect, res.StatusCode);
            Assert.NotNull(res.Headers.Location);

            if (include)
            {
                Assert.Contains("code_challenge=", res.Headers.Location.Query);
                Assert.Contains("code_challenge_method=S256", res.Headers.Location.Query);
            }
            else
            {
                Assert.DoesNotContain("code_challenge=", res.Headers.Location.Query);
                Assert.DoesNotContain("code_challenge_method=", res.Headers.Location.Query);
            }
        }

        [Theory]
        [InlineData(OpenIdConnectResponseType.Token)]
        [InlineData(OpenIdConnectResponseType.IdToken)]
        [InlineData(OpenIdConnectResponseType.CodeIdToken)]
        public async Task ChallengeDoesNotIncludePkceForOtherResponseTypes(string responseType)
        {
            var options = new OpenIdConnectAuthenticationOptions()
                          {
                              Authority = "https://authserver/",
                              ClientId = "Test Client Id",
                              ClientSecret = "Test Client Secret",
                              UsePkce = true,
                              ResponseType = responseType,
                              Configuration = new OpenIdConnectConfiguration()
                              {
                                  AuthorizationEndpoint = "https://authserver/auth"
                              }
                          };
            var server = CreateServer(
                app => app.UseOpenIdConnectAuthentication(options),
                context =>
                {
                    context.Authentication.Challenge("OpenIdConnect");
                    return true;
                });

            var transaction = await SendAsync(server, "http://example.com/challenge");

            var res = transaction.Response;
            Assert.Equal(HttpStatusCode.Redirect, res.StatusCode);
            Assert.NotNull(res.Headers.Location);

            Assert.DoesNotContain("code_challenge=", res.Headers.Location.Query);
            Assert.DoesNotContain("code_challenge_method=", res.Headers.Location.Query);
        }


        private static TestServer CreateServer(Action<IAppBuilder> configure, Func<IOwinContext, bool> handler)
        {
            return TestServer.Create(app =>
            {
                app.Properties["host.AppName"] = "OpenIdConnect.Owin.Security.Tests";
                app.UseCookieAuthentication(new CookieAuthenticationOptions
                {
                    AuthenticationType = "External"
                });
                app.SetDefaultSignInAsAuthenticationType("External");
                if (configure != null)
                {
                    configure(app);
                }
                app.Use(async (context, next) =>
                {
                    if (handler == null || !handler(context))
                    {
                        await next();
                    }
                });
            });
        }

        private static async Task<Transaction> SendAsync(TestServer server, string uri, string cookieHeader = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            if (!string.IsNullOrEmpty(cookieHeader))
            {
                request.Headers.Add("Cookie", cookieHeader);
            }
            var transaction = new Transaction
            {
                Request = request,
                Response = await server.HttpClient.SendAsync(request),
            };
            if (transaction.Response.Headers.Contains("Set-Cookie"))
            {
                transaction.SetCookie = transaction.Response.Headers.GetValues("Set-Cookie").ToList();
            }
            transaction.ResponseText = await transaction.Response.Content.ReadAsStringAsync();

            if (transaction.Response.Content != null &&
                transaction.Response.Content.Headers.ContentType != null &&
                transaction.Response.Content.Headers.ContentType.MediaType == "text/xml")
            {
                transaction.ResponseElement = XElement.Parse(transaction.ResponseText);
            }
            return transaction;
        }

        private class Transaction
        {
            public HttpRequestMessage Request { get; set; }
            public HttpResponseMessage Response { get; set; }
            public IList<string> SetCookie { get; set; }
            public string ResponseText { get; set; }
            public XElement ResponseElement { get; set; }

            public string AuthenticationCookieValue
            {
                get
                {
                    if (SetCookie != null && SetCookie.Count > 0)
                    {
                        var authCookie = SetCookie.SingleOrDefault(c => c.Contains(".AspNet.External="));
                        if (authCookie != null)
                        {
                            return authCookie.Substring(0, authCookie.IndexOf(';'));
                        }
                    }

                    return null;
                }
            }

            public string FindClaimValue(string claimType)
            {
                XElement claim = ResponseElement.Elements("claim").SingleOrDefault(elt => elt.Attribute("type").Value == claimType);
                if (claim == null)
                {
                    return null;
                }
                return claim.Attribute("value").Value;
            }
        }
    }
}
