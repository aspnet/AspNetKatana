// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Runtime.Serialization;
using System.Security.Authentication;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FunctionalTests.Common;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Twitter;
using Microsoft.Owin.Security.Twitter.Messages;
using Owin;
using Xunit;
using Xunit.Extensions;

namespace FunctionalTests.Facts.Security.Twitter
{
    public class TwitterAuthentication
    {
        [Theory, Trait("FunctionalTests", "Security")]
        [InlineData(HostType.HttpListener)]
        public async Task Security_TwitterAuthenticationWithProvider(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                string applicationUrl = deployer.Deploy(hostType, TwitterAuthenticationWithProviderConfiguration);

                var handler = new HttpClientHandler() { AllowAutoRedirect = false };
                var httpClient = new HttpClient(handler);
                httpClient.Timeout = new TimeSpan(0, 15, 0);

                // Unauthenticated request - verify Redirect url
                var response = await httpClient.GetAsync(applicationUrl);
                Assert.Equal<string>("https://api.twitter.com/oauth/authenticate", response.Headers.Location.AbsoluteUri.Replace(response.Headers.Location.Query, string.Empty));
                var queryItems = response.Headers.Location.ParseQueryString();
                Assert.Equal<string>("custom", queryItems["custom_redirect_uri"]);
                Assert.NotNull(queryItems["oauth_token"]);
                Assert.NotNull(handler.CookieContainer.GetCookies(new Uri(applicationUrl))["__TwitterState"]);

                //This is just to generate a correlation cookie. Previous step would generate this cookie, but we have reset the handler now.
                httpClient = new HttpClient(handler = new HttpClientHandler());
                response = await httpClient.GetAsync(applicationUrl);

                //Both oauth_token & oauth_verifier verifier missing - Expect an internal error
                response = await httpClient.GetAsync(GetTwitterSignInMockData(applicationUrl, oauth_token: null, oauth_verifier: null));
                Assert.Equal<HttpStatusCode>(HttpStatusCode.InternalServerError, response.StatusCode);
                Assert.Equal<string>("SignIn_Failed", await response.Content.ReadAsStringAsync());

                //Invalid oauth_token
                response = await httpClient.GetAsync(GetTwitterSignInMockData(applicationUrl, oauth_token: "invalid_oauth_token", oauth_verifier: "valid_oauth_verifier"));
                Assert.Equal<HttpStatusCode>(HttpStatusCode.InternalServerError, response.StatusCode);
                Assert.Equal<string>("SignIn_Failed", await response.Content.ReadAsStringAsync());

                //Valid oauth_token & invalid oauth_verifier
                response = await httpClient.GetAsync(GetTwitterSignInMockData(applicationUrl, oauth_verifier: "invalid_oauth_verifier"));
                Assert.Equal<HttpStatusCode>(HttpStatusCode.InternalServerError, response.StatusCode);
                Assert.Equal<string>("SignIn_Failed", await response.Content.ReadAsStringAsync());

                //Valid oauth_token & valid oauth_verifier
                response = await httpClient.GetAsync(GetTwitterSignInMockData(applicationUrl));
                Assert.Equal<HttpStatusCode>(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal<string>(response.RequestMessage.RequestUri.AbsoluteUri, applicationUrl);
                Assert.Equal<string>("Twitter", await response.Content.ReadAsStringAsync());
                Assert.NotNull(handler.CookieContainer.GetCookies(new Uri(applicationUrl))[".AspNet.Application"]);

                //Retry multiple times with valid cookie to test sliding expiration
                for (int retryCount = 0; retryCount < 3; retryCount++)
                {
                    response = await httpClient.GetAsync(applicationUrl);
                    Assert.Equal<string>(response.RequestMessage.RequestUri.AbsoluteUri, applicationUrl);
                }

                //Trigger cert validation error
                httpClient = new HttpClient(handler = new HttpClientHandler());
                response = await httpClient.GetAsync(applicationUrl);
                response = await httpClient.GetAsync(GetTwitterSignInMockData(applicationUrl, oauth_verifier: "InvalidCert"));
                Assert.Equal<HttpStatusCode>(HttpStatusCode.InternalServerError, response.StatusCode);
                Assert.Equal<string>("SignIn_Failed", await response.Content.ReadAsStringAsync());
            }
        }

        private string GetTwitterSignInMockData(string applicationUrl, string oauth_token = "valid_oauth_token", string oauth_verifier = "valid_oauth_verifier")
        {
            List<string> queryParameters = new List<string>();
            if (!string.IsNullOrWhiteSpace(oauth_token))
            {
                queryParameters.Add(string.Format("oauth_token={0}", oauth_token));
            }

            if (!string.IsNullOrWhiteSpace(oauth_verifier))
            {
                queryParameters.Add(string.Format("oauth_verifier={0}", oauth_verifier));
            }

            return new UriBuilder(applicationUrl + "signin-twitter") { Query = string.Join("&", queryParameters.ToArray()) }.Uri.AbsoluteUri;
        }

        public void TwitterAuthenticationWithProviderConfiguration(IAppBuilder app)
        {
            app.UseAuthSignInCookie();

            app.UseTwitterAuthentication(new TwitterAuthenticationOptions()
                {
                    ConsumerKey = "sgdtlH5fVziF5rAsivNZA",
                    ConsumerSecret = "lZLT7gEDcBgMrS9lIVzzPUdg61PoJVwfrOlMngaOhg",
                    Provider = new TwitterAuthenticationProvider()
                    {
                        OnAuthenticated = async context =>
                        {
                            await Task.Run(() =>
                                {
                                    Assert.Equal<string>("valid_user_id", context.UserId);
                                    Assert.Equal<string>("valid_screen_name", context.ScreenName);
                                    Assert.Equal<string>("valid_oauth_token", context.AccessToken);
                                    Assert.Equal<string>("valid_oauth_token_secret", context.AccessTokenSecret);
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
                                        context.Identity = new ClaimsIdentity("Twitter", "Name_Failed", "Role_Failed");
                                        context.SignInAsAuthenticationType = "Application";
                                    }
                                });
                        },
                    OnApplyRedirect = context =>
                        {
                            context.Response.Redirect(context.RedirectUri + "&custom_redirect_uri=custom");
                        }
                    },
                    StateDataFormat = new CustomTwitterRequestTokenFormat(),
                    BackchannelHttpHandler = new TwitterChannelHttpHandler(),
                    BackchannelCertificateValidator = new TwitterBackChannelCertificateValidator()
                });

            app.UseExternalApplication("Twitter");
        }
    }

    public class CustomTwitterRequestTokenFormat : ISecureDataFormat<RequestToken>
    {
        private static string lastSavedRequestToken;
        private DataContractSerializer serializer = new DataContractSerializer(typeof(RequestToken));

        public string Protect(RequestToken data)
        {
            data.Token = "valid_oauth_token";
            lastSavedRequestToken = Serialize(data);
            return "valid_oauth_token";
        }

        public RequestToken Unprotect(string state)
        {
            return state == "valid_oauth_token" ? DeSerialize(lastSavedRequestToken) : null;
        }

        private string Serialize(RequestToken data)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                serializer.WriteObject(memoryStream, data);
                memoryStream.Position = 0;
                return new StreamReader(memoryStream).ReadToEnd();
            }
        }

        private RequestToken DeSerialize(string state)
        {
            var stateDataAsBytes = Encoding.UTF8.GetBytes(state);

            using (var ms = new MemoryStream(stateDataAsBytes, false))
            {
                return (RequestToken)serializer.ReadObject(ms);
            }
        }
    }

    public class TwitterChannelHttpHandler : WebRequestHandler
    {
        private static bool RequestTokenEndpointInvoked = false;

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage();

            if (request.RequestUri.AbsoluteUri.StartsWith("https://api.twitter.com/oauth/access_token"))
            {
                var formData = request.Content.ReadAsFormDataAsync().Result;
                if (formData["oauth_verifier"] == "valid_oauth_verifier")
                {
                    if (RequestTokenEndpointInvoked)
                    {
                        var response_Form_data = new List<KeyValuePair<string, string>>()
                            {
                                new KeyValuePair<string, string>("oauth_token", "valid_oauth_token"),
                                new KeyValuePair<string, string>("oauth_token_secret", "valid_oauth_token_secret"),
                                new KeyValuePair<string, string>("user_id", "valid_user_id"),
                                new KeyValuePair<string, string>("screen_name", "valid_screen_name"),
                            };

                        response.Content = new FormUrlEncodedContent(response_Form_data);
                    }
                    else
                    {
                        response.StatusCode = HttpStatusCode.InternalServerError;
                        response.Content = new StringContent("RequestTokenEndpoint is not invoked");
                    }
                }
                else if (formData["oauth_verifier"] == "invalid_oauth_verifier")
                {
                    //TODO: How do we get status code back from Twitter?
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    response.Content = new StringContent("Cannot verify the oauth_token");
                }
                else if (formData["oauth_verifier"] == "InvalidCert")
                {
                    //Trigger cert validation failure.
                    request.Headers.Add("InvalidCert", true.ToString());

                    try
                    {
                        //This is to trigger the cert validation. 
                        response = await base.SendAsync(request, cancellationToken);
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

                    response.StatusCode = HttpStatusCode.OK;
                }
            }
            else if (request.RequestUri.AbsoluteUri.StartsWith("https://api.twitter.com/oauth/request_token"))
            {
                RequestTokenEndpointInvoked = true;
                return await base.SendAsync(request, cancellationToken);
            }

            return response;
        }
    }

    public class TwitterBackChannelCertificateValidator : ICertificateValidator
    {
        public bool Validate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            var requestHeaders = ((HttpWebRequest)sender).Headers;
            if (requestHeaders["InvalidCert"] != null)
            {
                return !bool.Parse(requestHeaders["InvalidCert"]);
            }
            else
            {
                return true;
            }
        }
    }
}