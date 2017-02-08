// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FunctionalTests.Common;
using FunctionalTests.Facts.Security.Common;
using Microsoft.Owin.Security.Google;
using Owin;
using Xunit;
using Xunit.Extensions;

namespace FunctionalTests.Facts.Security.Google
{
    public class GoogleOauth2Authentication
    {
        [Theory, Trait("FunctionalTests", "Security")]
        [InlineData(HostType.IIS)]
        [InlineData(HostType.HttpListener)]
        public async Task Security_GoogleOAuth2WithProvider(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                string applicationUrl = deployer.Deploy(hostType, GoogleOAuth2Configuration);
                var handler = new HttpClientHandler() { AllowAutoRedirect = false };
                var httpClient = new HttpClient(handler);

                // Unauthenticated request - verify Redirect url
                var response = await httpClient.GetAsync(applicationUrl);
                Assert.Equal<string>("https://accounts.google.com/o/oauth2/auth", response.Headers.Location.AbsoluteUri.Replace(response.Headers.Location.Query, string.Empty));
                var queryItems = response.Headers.Location.ParseQueryString();
                Assert.Equal<string>("code", queryItems["response_type"]);
                Assert.Equal<string>("offline", queryItems["access_type"]);
                Assert.Equal<string>("581497791735-f9317hcnvcrg9cvl1jfc3tev7teqfump.apps.googleusercontent.com", queryItems["client_id"]);
                Assert.Equal<string>(applicationUrl + "signin-google", queryItems["redirect_uri"]);
                Assert.Equal<string>("openid profile email", queryItems["scope"]);
                Assert.Equal<string>("ValidStateData", queryItems["state"]);
                Assert.Equal<string>("custom", queryItems["custom_redirect_uri"]);

                //This is just to generate a correlation cookie. Previous step would generate this cookie, but we have reset the handler now.
                httpClient = new HttpClient(handler = new HttpClientHandler());
                response = await httpClient.GetAsync(applicationUrl);
                var correlationCookie = handler.CookieContainer.GetCookies(new Uri(applicationUrl))[".AspNet.Correlation.Google"];
                Assert.NotNull(correlationCookie);

                //Invalid state, but valid code
                response = await httpClient.GetAsync(GetMockData(applicationUrl, state: "InvalidStateData"));
                Assert.Equal<HttpStatusCode>(HttpStatusCode.InternalServerError, response.StatusCode);
                Assert.Null(handler.CookieContainer.GetCookies(new Uri(applicationUrl))[".AspNet.Application"]);
                Assert.NotNull(handler.CookieContainer.GetCookies(new Uri(applicationUrl))[".AspNet.Correlation.Google"]);

                //Valid state, but missing code
                handler.CookieContainer.Add(correlationCookie);
                response = await httpClient.GetAsync(GetMockData(applicationUrl, code: null));
                Assert.Equal<HttpStatusCode>(HttpStatusCode.InternalServerError, response.StatusCode);
                Assert.Null(handler.CookieContainer.GetCookies(new Uri(applicationUrl))[".AspNet.Application"]);

                //Valid code & Valid state
                //handler.CookieContainer.Add(correlationCookie);
                response = await httpClient.GetAsync(GetMockData(applicationUrl));
                Assert.Equal<string>("Google", response.Content.ReadAsStringAsync().Result);
                var cookies = handler.CookieContainer.GetCookies(new Uri(applicationUrl));
                Assert.NotNull(cookies[".AspNet.Application"]);
                Assert.Null(handler.CookieContainer.GetCookies(new Uri(applicationUrl))[".AspNet.Correlation.Google"]);

                //Retry with valid credentials for a few times
                for (int retry = 0; retry < 4; retry++)
                {
                    response = await httpClient.GetAsync(applicationUrl);
                    Assert.Equal<string>("Google", await response.Content.ReadAsStringAsync());
                }

                //Valid state, but invalid code
                httpClient = new HttpClient(handler = new HttpClientHandler());
                response = await httpClient.GetAsync(applicationUrl);
                response = await httpClient.GetAsync(GetMockData(applicationUrl, code: "InvalidCode"));
                Assert.Equal<HttpStatusCode>(HttpStatusCode.InternalServerError, response.StatusCode);
                Assert.Equal<string>("SignIn_Failed", await response.Content.ReadAsStringAsync());

                //Valid state, trigger CertValidator
                httpClient = new HttpClient(handler = new HttpClientHandler());
                response = await httpClient.GetAsync(applicationUrl);
                response = await httpClient.GetAsync(GetMockData(applicationUrl, code: "InvalidCert"));
                Assert.Equal<HttpStatusCode>(HttpStatusCode.InternalServerError, response.StatusCode);
                Assert.Equal<string>("SignIn_Failed", await response.Content.ReadAsStringAsync());
            }
        }

        private string GetMockData(string applicationUrl, string code = "ValidCode", string state = "ValidStateData")
        {
            List<string> queryParameters = new List<string>();

            Action<string> AppendToQueryIfNotNull = (value) =>
            {
                if (!string.IsNullOrWhiteSpace(code))
                {
                    queryParameters.Add(value);
                }
            };

            AppendToQueryIfNotNull(string.Format("code={0}", code));
            AppendToQueryIfNotNull(string.Format("state={0}", state));
            return new UriBuilder(applicationUrl + "signin-google") { Query = string.Join("&", queryParameters.ToArray()) }.Uri.AbsoluteUri;
        }

        public void GoogleOAuth2Configuration(IAppBuilder app)
        {
            app.UseAuthSignInCookie();

            var option = new GoogleOAuth2AuthenticationOptions()
            {
                ClientId = "581497791735-f9317hcnvcrg9cvl1jfc3tev7teqfump.apps.googleusercontent.com",
                ClientSecret = "51LHrC4QaudgKrOQbkfEtz9P",
                AccessType = "offline",
                Provider = new GoogleOAuth2AuthenticationProvider()
                {
                    OnAuthenticated = async context =>
                        {
                            await Task.Run(() =>
                            {
                                if (context.Identity != null)
                                {
                                    Assert.Equal<string>("ValidAccessToken", context.AccessToken);
                                    Assert.Equal<string>("ValidRefreshToken", context.RefreshToken);
                                    Assert.Equal<string>("owinauthtester2@gmail.com", context.Email);
                                    Assert.Equal<string>("106790274378320830963", context.Id);
                                    Assert.Equal<string>("owinauthtester2", context.FamilyName);
                                    Assert.Equal<string>("owinauthtester2 owinauthtester2", context.Name);
                                    Assert.Equal<TimeSpan>(TimeSpan.FromSeconds(1200), context.ExpiresIn.Value);
                                    Assert.NotNull(context.User);
                                    context.Identity.AddClaim(new Claim("Authenticated", "true"));
                                }
                            });
                        },
                    OnReturnEndpoint = async context =>
                        {
                            await Task.Run(() =>
                            {
                                if (context.Identity != null && context.SignInAsAuthenticationType == "Application")
                                {
                                    context.Identity.AddClaim(new Claim("ReturnEndpoint", "true"));
                                    context.Identity.AddClaim(new Claim(context.Identity.RoleClaimType, "Guest", ClaimValueTypes.String));
                                }
                                else if (context.Identity == null)
                                {
                                    context.Identity = new ClaimsIdentity("Google", "Name_Failed", "Role_Failed");
                                    context.SignInAsAuthenticationType = "Application";
                                }
                            });
                        },
                    OnApplyRedirect = context =>
                        {
                            context.Response.Redirect(context.RedirectUri + "&custom_redirect_uri=custom");
                        }
                },
                BackchannelHttpHandler = new GoogleOAuth2ChannelHttpHandler(),
                BackchannelCertificateValidator = new CustomCertificateValidator(),
                StateDataFormat = new CustomStateDataFormat()
            };

            app.UseGoogleAuthentication(option);
            app.UseExternalApplication("Google");
        }
    }

    public class GoogleOAuth2ChannelHttpHandler : WebRequestHandler
    {
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage();

            if (request.RequestUri.AbsoluteUri.StartsWith("https://accounts.google.com/o/oauth2/token"))
            {
                var formData = await request.Content.ReadAsFormDataAsync();
                if (formData["grant_type"] == "authorization_code")
                {
                    if (formData["code"] == "ValidCode")
                    {
                        if (formData["redirect_uri"] != null && formData["redirect_uri"].EndsWith("signin-google") &&
                           formData["client_id"] == "581497791735-f9317hcnvcrg9cvl1jfc3tev7teqfump.apps.googleusercontent.com" && formData["client_secret"] == "51LHrC4QaudgKrOQbkfEtz9P")
                        {
                            response.Content = new StringContent("{\"access_token\":\"ValidAccessToken\",\"refresh_token\":\"ValidRefreshToken\",\"token_type\":\"Bearer\",\"expires_in\":\"1200\",\"id_token\":\"Token\"}", Encoding.UTF8, "application/json");
                        }
                    }
                    else if (formData["code"] == "InvalidCert")
                    {
                        //Trigger cert validation failure.
                        request.Headers.Add("InvalidCert", true.ToString());

                        try
                        {
                            //This is to trigger the cert validation. 
                            var certValidator = base.SendAsync(request, cancellationToken).Result;
                        }
                        catch (Exception exception)
                        {
                            while (exception != null && exception.GetType() != typeof(AuthenticationException))
                            {
                                exception = exception.InnerException;
                            }

                            if (exception.GetType() == typeof(AuthenticationException))
                            {
                                //Client will infer the failure if there is an exception
                                throw;
                            }
                        }
                    }
                    else if (formData["code"] == "InvalidCode")
                    {
                        response.Content = new StringContent("{\"error\":{\"message\":\"Invalid verification code format.\",\"type\":\"OAuthException\",\"code\":100}}", Encoding.UTF8, "text/javascript");
                    }
                }
            }
            else if (request.RequestUri.AbsoluteUri.StartsWith("https://www.googleapis.com/plus/v1/people/me"))
            {
                if (request.Headers.Authorization.Parameter == "ValidAccessToken")
                {
                    response.Content = new StringContent("{ \"kind\": \"plus#person\",\n \"etag\": \"\\\"YFr-hUROXQN7IOa3dUHg9dQ8eq0/2hY18HdHEP8NLykSTVEiAhkKsBE\\\"\",\n \"gender\": \"male\",\n \"emails\": [\n  {\n   \"value\": \"owinauthtester2@gmail.com\",\n   \"type\": \"account\"\n  }\n ],\n \"objectType\": \"person\",\n \"id\": \"106790274378320830963\",\n \"displayName\": \"owinauthtester2 owinauthtester2\",\n \"name\": {\n  \"familyName\": \"owinauthtester2\",\n  \"givenName\": \"FirstName\"\n },\n \"url\": \"https://plus.google.com/106790274378320830963\",\n \"image\": {\n  \"url\": \"https://lh3.googleusercontent.com/-XdUIqdMkCWA/AAAAAAAAAAI/AAAAAAAAAAA/4252rscbv5M/photo.jpg?sz=50\"\n },\n \"isPlusUser\": true,\n \"language\": \"en\",\n \"circledByCount\": 0,\n \"verified\": false\n}\n", Encoding.UTF8, "application/json");
                }
                else
                {
                    response.Content = new StringContent("{\"error\":{\"message\":\"Invalid OAuth access token.\",\"type\":\"OAuthException\",\"code\":190}}");
                }
            }

            return response;
        }
    }
}