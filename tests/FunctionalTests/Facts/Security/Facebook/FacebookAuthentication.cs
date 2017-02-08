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
using Microsoft.Owin.Security.Facebook;
using Owin;
using Xunit;
using Xunit.Extensions;

namespace FunctionalTests.Facts.Security.Facebook
{
    public class FacebookAuthentication
    {
        [Theory, Trait("FunctionalTests", "Security")]
        [InlineData(HostType.IIS)]
        [InlineData(HostType.HttpListener)]
        public async Task Security_FacebookAuthentication(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                var applicationUrl = deployer.Deploy(hostType, FacebookAuthenticationConfiguration);

                var handler = new HttpClientHandler() { AllowAutoRedirect = false };
                var httpClient = new HttpClient(handler);

                // Unauthenticated request - verify Redirect url
                var response = await httpClient.GetAsync(applicationUrl);
                Assert.Equal<string>("https://www.facebook.com/v2.8/dialog/oauth", response.Headers.Location.AbsoluteUri.Replace(response.Headers.Location.Query, string.Empty));
                var queryItems = response.Headers.Location.ParseQueryString();
                Assert.Equal<string>("code", queryItems["response_type"]);
                Assert.Equal<string>("550624398330273", queryItems["client_id"]);
                Assert.Equal<string>(applicationUrl + "signin-facebook", queryItems["redirect_uri"]);
                Assert.Equal<string>("email,read_friendlists,user_checkins", queryItems["scope"]);
                Assert.Equal<string>("ValidStateData", queryItems["state"]);
                Assert.Equal<string>("custom", queryItems["custom_redirect_uri"]);

                //This is just to generate a correlation cookie. Previous step would generate this cookie, but we have reset the handler now.
                httpClient = new HttpClient(handler = new HttpClientHandler());
                response = await httpClient.GetAsync(applicationUrl);
                var correlationCookie = handler.CookieContainer.GetCookies(new Uri(applicationUrl))[".AspNet.Correlation.Facebook"];
                Assert.NotNull(correlationCookie);

                //Invalid state, but valid code
                response = await httpClient.GetAsync(GetFacebookSignInMockData(applicationUrl, state: "InvalidStateData"));
                Assert.Equal<HttpStatusCode>(HttpStatusCode.InternalServerError, response.StatusCode);
                Assert.Null(handler.CookieContainer.GetCookies(new Uri(applicationUrl))[".AspNet.Application"]);
                Assert.NotNull(handler.CookieContainer.GetCookies(new Uri(applicationUrl))[".AspNet.Correlation.Facebook"]);

                //Valid state, but missing code
                handler.CookieContainer.Add(correlationCookie);
                response = await httpClient.GetAsync(GetFacebookSignInMockData(applicationUrl, code: null));
                Assert.Equal<HttpStatusCode>(HttpStatusCode.InternalServerError, response.StatusCode);
                Assert.Equal<string>("SignIn_Failed", await response.Content.ReadAsStringAsync());
                Assert.Null(handler.CookieContainer.GetCookies(new Uri(applicationUrl))[".AspNet.Correlation.Facebook"]);

                //Valid code & Valid state
                handler.CookieContainer.Add(correlationCookie);
                response = await httpClient.GetAsync(GetFacebookSignInMockData(applicationUrl));
                Assert.Equal<string>("Facebook", response.Content.ReadAsStringAsync().Result);
                var cookies = handler.CookieContainer.GetCookies(new Uri(applicationUrl));
                Assert.NotNull(cookies[".AspNet.Application"]);
                Assert.Null(handler.CookieContainer.GetCookies(new Uri(applicationUrl))[".AspNet.Correlation.Facebook"]);

                //Retry with valid credentials for a few times
                for (int retry = 0; retry < 4; retry++)
                {
                    response = await httpClient.GetAsync(applicationUrl);
                    Assert.Equal<string>("Facebook", response.Content.ReadAsStringAsync().Result);
                }

                //Valid state, but invalid code
                httpClient = new HttpClient(handler = new HttpClientHandler());
                response = await httpClient.GetAsync(applicationUrl);
                response = await httpClient.GetAsync(GetFacebookSignInMockData(applicationUrl, code: "InvalidCode"));
                Assert.Equal<HttpStatusCode>(HttpStatusCode.InternalServerError, response.StatusCode);
                Assert.Equal<string>("SignIn_Failed", response.Content.ReadAsStringAsync().Result);

                //Valid state, trigger CertValidator
                httpClient = new HttpClient(handler = new HttpClientHandler());
                response = await httpClient.GetAsync(applicationUrl);
                response = await httpClient.GetAsync(GetFacebookSignInMockData(applicationUrl, code: "InvalidCert"));
                Assert.Equal<HttpStatusCode>(HttpStatusCode.InternalServerError, response.StatusCode);
                Assert.Equal<string>("SignIn_Failed", response.Content.ReadAsStringAsync().Result);
            }
        }

        private static string GetFacebookSignInMockData(string applicationUrl, string code = "ValidCode", string state = "ValidStateData")
        {
            var queryParameters = new List<string>();

            if (!string.IsNullOrWhiteSpace(code))
            {
                queryParameters.Add(string.Format("code={0}", code));
            }

            if (!string.IsNullOrWhiteSpace(state))
            {
                queryParameters.Add(string.Format("state={0}", state));
            }

            return new UriBuilder(applicationUrl + "signin-facebook") { Query = string.Join("&", queryParameters.ToArray()) }.Uri.AbsoluteUri;
        }

        public void FacebookAuthenticationConfiguration(IAppBuilder app)
        {
            app.UseAuthSignInCookie();

            var option = new FacebookAuthenticationOptions()
            {
                AppId = "550624398330273",
                AppSecret = "10e56a291d6b618da61b1e0dae3a8954",
                Provider = new FacebookAuthenticationProvider()
                {
                    OnAuthenticated = async context =>
                    {
                        await Task.Run(() =>
                        {
                            if (context.Identity != null)
                            {
                                Assert.Equal<string>("ValidAccessToken", context.AccessToken);
                                Assert.Equal<string>("owinauthtester2@gmail.com", context.Email);
                                Assert.Equal<string>("Id", context.Id);
                                Assert.Equal<string>("https://www.facebook.com/myLink", context.Link);
                                Assert.Equal<string>("Owinauthtester Owinauthtester", context.Name);
                                Assert.Equal<string>("owinauthtester.owinauthtester.7", context.UserName);
                                Assert.Equal<string>(context.Id, context.User.SelectToken("id").ToString());
                                Assert.Equal<TimeSpan>(TimeSpan.FromSeconds(100), context.ExpiresIn.Value);
                                context.Identity.AddClaim(new Claim("Authenticated", "true"));
                            }
                        }
                    );
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
                                    context.Identity = new ClaimsIdentity("Facebook", "Name_Failed", "Role_Failed");
                                    context.SignInAsAuthenticationType = "Application";
                                }
                            });
                    },
                    OnApplyRedirect = context =>
                        {
                            context.Response.Redirect(context.RedirectUri + "&custom_redirect_uri=custom");
                        }
                },
                BackchannelHttpHandler = new FacebookChannelHttpHandler(),
                BackchannelCertificateValidator = new CustomCertificateValidator(),
                StateDataFormat = new CustomStateDataFormat()
            };

            option.Scope.Add("email");
            option.Scope.Add("read_friendlists");
            option.Scope.Add("user_checkins");

            app.UseFacebookAuthentication(option);
            app.UseExternalApplication("Facebook");
        }
    }

    public class FacebookChannelHttpHandler : WebRequestHandler
    {
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage();
            var queryParameters = request.RequestUri.ParseQueryString();

            if (request.RequestUri.AbsoluteUri.StartsWith("https://graph.facebook.com/v2.8/oauth/access_token"))
            {
                if (queryParameters["grant_type"] == "authorization_code")
                {
                    if (queryParameters["code"] == "ValidCode")
                    {
                        Assert.True(queryParameters["redirect_uri"].EndsWith("signin-facebook"), "Redirect URI is not ending with /signin-facebook");
                        Assert.Equal<string>("550624398330273", queryParameters["client_id"]);
                        Assert.Equal<string>("10e56a291d6b618da61b1e0dae3a8954", queryParameters["client_secret"]);
                        response.Content = new StringContent("{\"access_token\":\"ValidAccessToken\",\"token_type\":\"Bearer\",\"expires_in\":\"100\"}", Encoding.UTF8, "application/json");
                    }
                    else if (queryParameters["code"] == "InvalidCert")
                    {
                        //Trigger cert validation failure.
                        request.Headers.Add("InvalidCert", true.ToString());

                        try
                        {
                            //This is to trigger the cert validation. 
                            var certValidator = await base.SendAsync(request, cancellationToken);
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
                    else if (queryParameters["code"] == "InvalidCode")
                    {
                        response.Content = new StringContent("{\"error\":{\"message\":\"Invalid verification code format.\",\"type\":\"OAuthException\",\"code\":100}}", Encoding.UTF8, "text/javascript");
                    }
                }
            }
            else if (request.RequestUri.AbsoluteUri.StartsWith("https://graph.facebook.com/v2.8/me"))
            {
                Assert.NotEqual<string>(null, queryParameters["appsecret_proof"]);
                if (queryParameters["access_token"] == "ValidAccessToken")
                {
                    response.Content = new StringContent("{\"id\":\"Id\",\"name\":\"Owinauthtester Owinauthtester\",\"first_name\":\"Owinauthtester\",\"last_name\":\"Owinauthtester\",\"link\":\"https:\\/\\/www.facebook.com\\/myLink\",\"username\":\"owinauthtester.owinauthtester.7\",\"gender\":\"male\",\"email\":\"owinauthtester2\\u0040gmail.com\",\"timezone\":-7,\"locale\":\"en_US\",\"verified\":true,\"updated_time\":\"2013-08-06T20:38:48+0000\",\"CertValidatorInvoked\":\"ValidAccessToken\"}");
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