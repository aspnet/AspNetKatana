// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FunctionalTests.Common;
using FunctionalTests.Facts.Security.Common;
using Microsoft.Owin.Security.MicrosoftAccount;
using Owin;
using Xunit;
using Xunit.Extensions;

namespace FunctionalTests.Facts.Security.MicrosoftAccount
{
    public class MicrosoftAuthentication
    {
        [Theory, Trait("FunctionalTests", "Security")]
        [InlineData(HostType.HttpListener)]
        public async Task Security_MicrosoftAuthenticationWithProvider(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer("katanatesting.com"))
            {
                //Edit the hosts file at c:\Windows\System32\drivers\etc\hosts and append this at the end before running the test
                //#My entries
                //127.0.0.1 katanatesting.com
                var hostsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), @"drivers\etc\hosts");
                if (!File.ReadAllText(hostsFilePath).Contains("127.0.0.1 katanatesting.com"))
                {
                    File.AppendAllText(hostsFilePath, "127.0.0.1 katanatesting.com");
                }

                string applicationUrl = deployer.Deploy(hostType, MicrosoftAuthenticationWithProviderConfiguration);
                //Fix application Url hostname
                applicationUrl = new UriBuilder(applicationUrl) { Host = "katanatesting.com" }.Uri.AbsoluteUri;

                var handler = new HttpClientHandler() { AllowAutoRedirect = false };
                var httpClient = new HttpClient(handler);

                // Unauthenticated request - verify Redirect url
                var response = await httpClient.GetAsync(applicationUrl);
                Assert.Equal("https://login.microsoftonline.com/common/oauth2/v2.0/authorize", response.Headers.Location.AbsoluteUri.Replace(response.Headers.Location.Query, string.Empty));
                var queryItems = response.Headers.Location.ParseQueryString();
                Assert.Equal("code", queryItems["response_type"]);
                Assert.Equal("000000004C0F442C", queryItems["client_id"]);
                Assert.Equal(applicationUrl + "signin-microsoft", queryItems["redirect_uri"]);
                Assert.Equal("https://graph.microsoft.com/user.read", queryItems["scope"]);
                Assert.Equal("ValidStateData", queryItems["state"]);
                Assert.Equal("custom", queryItems["custom_redirect_uri"]);

                //This is just to generate a correlation cookie. Previous step would generate this cookie, but we have reset the handler now.
                httpClient = new HttpClient(handler = new HttpClientHandler());
                response = await httpClient.GetAsync(applicationUrl);
                var correlationCookie = handler.CookieContainer.GetCookies(new Uri(applicationUrl))[".AspNet.Correlation.Microsoft"];
                Assert.NotNull(correlationCookie);

                //Invalid state, but valid code
                response = await httpClient.GetAsync(GetMicrosoftSignInMockData(applicationUrl, state: "InvalidStateData"));
                Assert.Equal<HttpStatusCode>(HttpStatusCode.InternalServerError, response.StatusCode);
                Assert.Null(handler.CookieContainer.GetCookies(new Uri(applicationUrl))[".AspNet.Application"]);
                Assert.NotNull(handler.CookieContainer.GetCookies(new Uri(applicationUrl))[".AspNet.Correlation.Microsoft"]);

                //Valid state, but missing code
                handler.CookieContainer.Add(correlationCookie);
                response = await httpClient.GetAsync(GetMicrosoftSignInMockData(applicationUrl, code: null));
                Assert.Equal<HttpStatusCode>(HttpStatusCode.InternalServerError, response.StatusCode);
                Assert.Equal("SignIn_Failed", await response.Content.ReadAsStringAsync());
                Assert.Null(handler.CookieContainer.GetCookies(new Uri(applicationUrl))[".AspNet.Correlation.Microsoft"]);

                //Valid code & Valid state
                handler.CookieContainer.Add(correlationCookie);
                response = await httpClient.GetAsync(GetMicrosoftSignInMockData(applicationUrl));
                Assert.Equal("Microsoft", await response.Content.ReadAsStringAsync());
                var cookies = handler.CookieContainer.GetCookies(new Uri(applicationUrl));
                Assert.NotNull(cookies[".AspNet.Application"]);
                Assert.Null(handler.CookieContainer.GetCookies(new Uri(applicationUrl))[".AspNet.Correlation.Microsoft"]);

                //Retry with valid credentials for a few times
                for (int retry = 0; retry < 4; retry++)
                {
                    response = await httpClient.GetAsync(applicationUrl);
                    Assert.Equal("Microsoft", await response.Content.ReadAsStringAsync());
                }

                //Valid state, but invalid code
                httpClient = new HttpClient(handler = new HttpClientHandler());
                response = await httpClient.GetAsync(applicationUrl);
                response = await httpClient.GetAsync(GetMicrosoftSignInMockData(applicationUrl, code: "InvalidCode"));
                Assert.Equal<HttpStatusCode>(HttpStatusCode.InternalServerError, response.StatusCode);
                Assert.Equal("SignIn_Failed", await response.Content.ReadAsStringAsync());

                //Valid state, trigger CertValidator
                httpClient = new HttpClient(handler = new HttpClientHandler());
                response = await httpClient.GetAsync(applicationUrl);
                response = await httpClient.GetAsync(GetMicrosoftSignInMockData(applicationUrl, code: "InvalidCert"));
                Assert.Equal<HttpStatusCode>(HttpStatusCode.InternalServerError, response.StatusCode);
                Assert.Equal("SignIn_Failed", await response.Content.ReadAsStringAsync());
            }
        }

        private string GetMicrosoftSignInMockData(string applicationUrl, string code = "ValidCode", string state = "ValidStateData")
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

            return new UriBuilder(applicationUrl + "signin-microsoft") { Query = string.Join("&", queryParameters.ToArray()) }.Uri.AbsoluteUri;
        }

        internal void MicrosoftAuthenticationWithProviderConfiguration(IAppBuilder app)
        {
            app.UseAuthSignInCookie();

            var option = new MicrosoftAccountAuthenticationOptions()
            {
                ClientId = "000000004C0F442C",
                // [SuppressMessage("Microsoft.Security", "CS002:SecretInNextLine", Justification="Unit test dummy credentials.")]
                ClientSecret = "EkXbW-Vr6Rqzi6pugl1jWIBsDotKLmqR",
                Provider = new MicrosoftAccountAuthenticationProvider()
                {
                    OnAuthenticated = async context =>
                    {
                        await Task.Run(() =>
                            {
                                Assert.Equal("ValidAccessToken", context.AccessToken);
                                Assert.Equal("ValidRefreshToken", context.RefreshToken);
                                Assert.Equal("Owinauthtester", context.FirstName);
                                Assert.Equal("fccf9a24999f4f4f", context.Id);
                                Assert.Equal("Owinauthtester", context.LastName);
                                Assert.Equal("Owinauthtester Owinauthtester", context.Name);
                                Assert.NotNull(context.User);
                                Assert.Equal(context.Id, context.User.SelectToken("id").ToString());
                                context.Identity.AddClaim(new Claim("Authenticated", "true"));
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
                                    context.Identity = new ClaimsIdentity("Microsoft", "Name_Failed", "Role_Failed");
                                    context.SignInAsAuthenticationType = "Application";
                                }
                            });
                    },
                    OnApplyRedirect = context =>
                        {
                            context.Response.Redirect(context.RedirectUri + "&custom_redirect_uri=custom");
                        }
                },
                BackchannelHttpHandler = new MicrosoftChannelHttpHandler(),
                BackchannelCertificateValidator = new CustomCertificateValidator(),
                StateDataFormat = new CustomStateDataFormat(),
            };

            app.UseMicrosoftAccountAuthentication(option);
            app.UseExternalApplication("Microsoft");
        }
    }

    public class MicrosoftChannelHttpHandler : WebRequestHandler
    {
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage()
            {
                Content = new StringContent("")
            };

            if (request.RequestUri.AbsoluteUri.StartsWith("https://login.microsoftonline.com/common/oauth2/v2.0/token"))
            {
                var formData = request.Content.ReadAsFormDataAsync().Result;

                if (formData["grant_type"] == "authorization_code")
                {
                    if (formData["code"] == "ValidCode")
                    {
                        if (formData["redirect_uri"] != null && formData["redirect_uri"].EndsWith("signin-microsoft") &&
                           formData["client_id"] == "000000004C0F442C" && formData["client_secret"] == "EkXbW-Vr6Rqzi6pugl1jWIBsDotKLmqR")
                        {
                            response.Content = new StringContent("{\"token_type\":\"bearer\",\"expires_in\":3600,\"scope\":\"https://graph.microsoft.com/user.read\",\"access_token\":\"ValidAccessToken\",\"refresh_token\":\"ValidRefreshToken\",\"authentication_token\":\"ValidAuthenticationToken\"}");
                        }
                    }
                    else if (formData["code"] == "InvalidCert")
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
                    else if (formData["code"] == "InvalidCode")
                    {
                        response.Content = new StringContent("{\"error\":\"invalid_request\",\"error_description\":\"The provided request must include a 'code' input parameter.\"}", Encoding.UTF8, "text/javascript");
                    }
                }
            }
            else if (request.RequestUri.AbsoluteUri.StartsWith("https://graph.microsoft.com/v1.0/me"))
            {
                if (request.Headers.Authorization.Parameter == "ValidAccessToken")
                {
                    response.Content = new StringContent("{\r   \"id\": \"fccf9a24999f4f4f\", \r   \"displayName\": \"Owinauthtester Owinauthtester\", \r   \"givenName\": \"Owinauthtester\", \r   \"surname\": \"Owinauthtester\", \r   \"link\": \"https://profile.live.com/\", \r   \"gender\": null, \r   \"locale\": \"en_US\", \r   \"updated_time\": \"2013-08-27T22:18:14+0000\"\r}");
                }
                else
                {
                    response.Content = new StringContent("{\r   \"error\": {\r      \"code\": \"request_token_invalid\", \r      \"message\": \"The access token isn't valid.\"\r   }\r}", Encoding.UTF8, "text/javascript");
                }
            }

            return response;
        }
    }
}