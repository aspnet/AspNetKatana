// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using FunctionalTests.Common;
using Microsoft.Owin;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.Security.Infrastructure;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json.Linq;
using Owin;
using Xunit;
using Xunit.Extensions;
using kvp = System.Collections.Generic.KeyValuePair<string, string>;

namespace FunctionalTests.Facts.Security.AuthServer
{
    public class AuthServerTests
    {
        private readonly ConcurrentDictionary<string, string> _authenticationCodes = new ConcurrentDictionary<string, string>(StringComparer.Ordinal);
        private readonly ConcurrentDictionary<string, string> _refreshTokens = new ConcurrentDictionary<string, string>(StringComparer.Ordinal);
        const string Client_Redirect_Uri = "http://localhost:5555/redirectUri";

        [Theory, Trait("FunctionalTests", "Security")]
        [InlineData(HostType.IIS)]
        [InlineData(HostType.HttpListener)]
        public void Security_AuthorizeEndpointTests(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                var applicationUrl = deployer.Deploy(hostType, AuthServerHappyPathConfiguration);

                IDisposable clientEndpoint = null;
                try
                {
                    clientEndpoint = WebApp.Start(Client_Redirect_Uri, app => app.Run(context => { return context.Response.WriteAsync(context.Request.QueryString.Value); }));

                    string tokenEndpointUri = applicationUrl + "TokenEndpoint";
                    var basicClient = new HttpClient();
                    var headerValue = Convert.ToBase64String(Encoding.Default.GetBytes(string.Format("{0}:{1}", "123", "invalid")));
                    basicClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", headerValue);

                    HttpClient httpClient = new HttpClient();
                    string requestUri = null;
                    Uri landingUri = null;
                    Uri applicationUri = new Uri(applicationUrl);
                    HttpResponseMessage httpResponseMessage = null;

                    //Happy path - response_type:code
                    requestUri = AuthZ.CreateAuthZUri(applicationUrl, "code", "123", Client_Redirect_Uri, "scope1", "validstate");
                    landingUri = httpClient.GetAsync(requestUri).Result.RequestMessage.RequestUri;
                    Assert.Equal(Client_Redirect_Uri, landingUri.GetLeftPart(UriPartial.Path));
                    Assert.NotNull(landingUri.ParseQueryString()["code"]);
                    Assert.Equal("validstate", landingUri.ParseQueryString()["state"]);

                    //Happy path - response_type:token
                    requestUri = AuthZ.CreateAuthZUri(applicationUrl, "token", "123", Client_Redirect_Uri, "scope1", "validstate");
                    landingUri = httpClient.GetAsync(requestUri).Result.RequestMessage.RequestUri;
                    landingUri = new Uri(landingUri.AbsoluteUri.Replace('#', '?'));
                    Assert.Equal(Client_Redirect_Uri, landingUri.GetLeftPart(UriPartial.Path));
                    Assert.NotNull(landingUri.ParseQueryString()["access_token"]);
                    Assert.NotNull(landingUri.ParseQueryString()["expires_in"]);
                    Assert.Equal("bearer", landingUri.ParseQueryString()["token_type"]);
                    Assert.Equal("validstate", landingUri.ParseQueryString()["state"]);

                    //Invalid redirect URI - pass error to application
                    requestUri = AuthZ.CreateAuthZUri(applicationUrl, "code", "123", "invalid_uri_passonerror", "scope1", "validstate");
                    httpResponseMessage = httpClient.GetAsync(requestUri).Result;
                    Assert.Equal("error: invalid_request\r\n", httpResponseMessage.Content.ReadAsStringAsync().Result);
                    Assert.True(httpResponseMessage.RequestMessage.RequestUri.GetLeftPart(UriPartial.Authority).StartsWith(applicationUri.GetLeftPart(UriPartial.Authority)), "Should not be redirected on invalid redirect_uri");

                    //Invalid redirect URI - Display error by middleware
                    requestUri = AuthZ.CreateAuthZUri(applicationUrl, "code", "123", "invalid_uri_displayerror", "scope1", "validstate");
                    httpResponseMessage = httpClient.GetAsync(requestUri).Result;
                    Assert.True(httpResponseMessage.RequestMessage.RequestUri.GetLeftPart(UriPartial.Authority).StartsWith(applicationUri.GetLeftPart(UriPartial.Authority)), "Should not be redirected on invalid redirect_uri");
                    Assert.True(httpResponseMessage.Content.ReadAsStringAsync().Result.StartsWith("error: invalid_request"), "Did not receive an error for an invalid redirect_uri");

                    //What happens if we don't set Validated explicitly. Send an invalid clientId => We don't set Validated for this case.
                    requestUri = AuthZ.CreateAuthZUri(applicationUrl, "token", "invalidClient", Client_Redirect_Uri, "scope1", "validstate");
                    httpResponseMessage = httpClient.GetAsync(requestUri).Result;
                    Assert.True(httpResponseMessage.RequestMessage.RequestUri.GetLeftPart(UriPartial.Authority).StartsWith(applicationUri.GetLeftPart(UriPartial.Authority)), "Should not be redirected on invalid redirect_uri");
                    Assert.True(httpResponseMessage.Content.ReadAsStringAsync().Result.StartsWith("error: invalid_request"), "Did not receive an error for an invalid redirect_uri");

                    //OnValidateAuthorizeRequest - Rejecting a request. Send an invalid state as we validate it there. Client should receive all the error code & description that we send
                    requestUri = AuthZ.CreateAuthZUri(applicationUrl, "code", "123", Client_Redirect_Uri, "scope1", "invalidstate");
                    httpResponseMessage = httpClient.GetAsync(requestUri).Result;
                    landingUri = httpResponseMessage.RequestMessage.RequestUri;
                    Assert.Equal(Client_Redirect_Uri, landingUri.GetLeftPart(UriPartial.Path));
                    Assert.Equal("state.invalid", landingUri.ParseQueryString()["error"]);
                    Assert.Equal("state.invaliddescription", landingUri.ParseQueryString()["error_description"]);
                    Assert.Equal("state.invaliduri", landingUri.ParseQueryString()["error_uri"]);
                    Assert.True(httpResponseMessage.Content.ReadAsStringAsync().Result.StartsWith("error=state.invalid&error_description=state.invaliddescription&error_uri=state.invaliduri"), "Did not receive an error when provider did not set Validated");

                    //Missing response_type
                    requestUri = AuthZ.CreateAuthZUri(applicationUrl, null, "123", Client_Redirect_Uri, "scope1", "validstate");
                    httpResponseMessage = httpClient.GetAsync(requestUri).Result;
                    Assert.Equal(Client_Redirect_Uri, httpResponseMessage.RequestMessage.RequestUri.GetLeftPart(UriPartial.Path));
                    Assert.Equal("invalid_request", httpResponseMessage.RequestMessage.RequestUri.ParseQueryString()["error"]);

                    //Unsupported response_type
                    requestUri = AuthZ.CreateAuthZUri(applicationUrl, "invalid_response_type", "123", Client_Redirect_Uri, "scope1", "validstate");
                    httpResponseMessage = httpClient.GetAsync(requestUri).Result;
                    Assert.Equal(Client_Redirect_Uri, httpResponseMessage.RequestMessage.RequestUri.GetLeftPart(UriPartial.Path));
                    Assert.Equal("unsupported_response_type", httpResponseMessage.RequestMessage.RequestUri.ParseQueryString()["error"]);

                    //Missing client_id
                    requestUri = AuthZ.CreateAuthZUri(applicationUrl, "token", null, Client_Redirect_Uri, "scope1", "validstate");
                    httpResponseMessage = httpClient.GetAsync(requestUri).Result;
                    Assert.True(httpResponseMessage.RequestMessage.RequestUri.GetLeftPart(UriPartial.Authority).StartsWith(applicationUri.GetLeftPart(UriPartial.Authority)), "Should not be redirected on invalid redirect_uri");
                    Assert.True(httpResponseMessage.Content.ReadAsStringAsync().Result.StartsWith("error: invalid_request"), "Did not receive an error for an invalid redirect_uri");

                    //Missing state - Should succeed
                    requestUri = AuthZ.CreateAuthZUri(applicationUrl, "code", "123", Client_Redirect_Uri, "scope1", null);
                    landingUri = httpClient.GetAsync(requestUri).Result.RequestMessage.RequestUri;
                    Assert.Equal(Client_Redirect_Uri, landingUri.GetLeftPart(UriPartial.Path));
                    Assert.NotNull(landingUri.ParseQueryString()["code"]);
                    Assert.False(landingUri.ParseQueryString().ContainsKey("state"));

                    //Token endpoint tests
                    //Invalid client (client_id, client_secret) - As form parameters
                    var formContent = AuthZ.CreateTokenEndpointContent(new[] { new kvp("client_id", "123"), new kvp("client_secret", "invalid") });
                    var responseMessage = httpClient.PostAsync(tokenEndpointUri, formContent).Result.Content.ReadAsStringAsync().Result;
                    var jToken = JToken.Parse(responseMessage);
                    Assert.Equal("invalid_client", jToken.SelectToken("error").Value<string>());

                    //Invalid client (client_id, client_secret) - As Basic auth headers
                    responseMessage = basicClient.GetAsync(tokenEndpointUri).Result.Content.ReadAsStringAsync().Result;
                    jToken = JToken.Parse(responseMessage);
                    Assert.Equal("invalid_client", jToken.SelectToken("error").Value<string>());

                    //grant_type=authorization_code - invalid code being sent
                    formContent = AuthZ.CreateTokenEndpointContent(new[] { new kvp("client_id", "123"), new kvp("client_secret", "secret123"), new kvp("grant_type", "authorization_code"), new kvp("code", "InvalidCode"), new kvp("redirect_uri", Client_Redirect_Uri) });
                    responseMessage = httpClient.PostAsync(tokenEndpointUri, formContent).Result.Content.ReadAsStringAsync().Result;
                    jToken = JToken.Parse(responseMessage);
                    Assert.Equal("invalid_grant", jToken.SelectToken("error").Value<string>());

                    //grant_type=authorization_code - Full scenario
                    requestUri = AuthZ.CreateAuthZUri(applicationUrl, "code", "123", Client_Redirect_Uri, "scope1", "validstate");
                    landingUri = httpClient.GetAsync(requestUri).Result.RequestMessage.RequestUri;
                    Assert.Equal(Client_Redirect_Uri, landingUri.GetLeftPart(UriPartial.Path));
                    Assert.NotNull(landingUri.ParseQueryString()["code"]);
                    Assert.Equal("validstate", landingUri.ParseQueryString()["state"]);
                    formContent = AuthZ.CreateTokenEndpointContent(new[] { new kvp("client_id", "123"), new kvp("client_secret", "secret123"), new kvp("grant_type", "authorization_code"), new kvp("code", landingUri.ParseQueryString()["code"]), new kvp("redirect_uri", Client_Redirect_Uri) });
                    responseMessage = httpClient.PostAsync(tokenEndpointUri, formContent).Result.Content.ReadAsStringAsync().Result;
                    jToken = JToken.Parse(responseMessage);
                    Assert.NotNull(jToken.SelectToken("access_token").Value<string>());
                    Assert.Equal("bearer", jToken.SelectToken("token_type").Value<string>());
                    Assert.NotNull(jToken.SelectToken("expires_in").Value<string>());
                    Assert.Equal("value1", jToken.SelectToken("param1").Value<string>());
                    Assert.Equal("value2", jToken.SelectToken("param2").Value<string>());
                    Assert.NotNull(jToken.SelectToken("refresh_token").Value<string>());

                    //grant_type=password -- Resource owner credentials -- Invalid credentials
                    formContent = AuthZ.CreateTokenEndpointContent(new[] { new kvp("client_id", "123"), new kvp("client_secret", "secret123"), new kvp("grant_type", "password"), new kvp("username", "user1"), new kvp("password", "invalid"), new kvp("scope", "scope1 scope2 scope3") });
                    responseMessage = httpClient.PostAsync(tokenEndpointUri, formContent).Result.Content.ReadAsStringAsync().Result;
                    jToken = JToken.Parse(responseMessage);
                    Assert.Equal("invalid_grant", jToken.SelectToken("error").Value<string>());

                    //grant_type=password -- Resource owner credentials
                    formContent = AuthZ.CreateTokenEndpointContent(new[] { new kvp("client_id", "123"), new kvp("client_secret", "secret123"), new kvp("grant_type", "password"), new kvp("username", "user1"), new kvp("password", "password1"), new kvp("scope", "scope1 scope2 scope3") });
                    responseMessage = httpClient.PostAsync(tokenEndpointUri, formContent).Result.Content.ReadAsStringAsync().Result;
                    jToken = JToken.Parse(responseMessage);
                    Assert.NotNull(jToken.SelectToken("access_token").Value<string>());
                    Assert.Equal("bearer", jToken.SelectToken("token_type").Value<string>());
                    Assert.NotNull(jToken.SelectToken("expires_in").Value<string>());
                    Assert.Equal("value1", jToken.SelectToken("param1").Value<string>());
                    Assert.Equal("value2", jToken.SelectToken("param2").Value<string>());
                    Assert.NotNull(jToken.SelectToken("refresh_token").Value<string>());

                    //grant_type=refresh_token -- Use the refresh token issued on the previous call
                    formContent = AuthZ.CreateTokenEndpointContent(new[] { new kvp("client_id", "123"), new kvp("client_secret", "secret123"), new kvp("grant_type", "refresh_token"), new kvp("refresh_token", jToken.SelectToken("refresh_token").Value<string>()), new kvp("scope", "scope1 scope2") });
                    responseMessage = httpClient.PostAsync(tokenEndpointUri, formContent).Result.Content.ReadAsStringAsync().Result;
                    jToken = JToken.Parse(responseMessage);
                    Assert.NotNull(jToken.SelectToken("access_token").Value<string>());
                    Assert.Equal("bearer", jToken.SelectToken("token_type").Value<string>());
                    Assert.NotNull(jToken.SelectToken("expires_in").Value<string>());
                    Assert.Equal("value1", jToken.SelectToken("param1").Value<string>());
                    Assert.Equal("value2", jToken.SelectToken("param2").Value<string>());
                    Assert.NotNull(jToken.SelectToken("refresh_token").Value<string>());

                    //grant_type=client_credentials - Bug# https://github.com/Katana/katana/issues/562
                    formContent = AuthZ.CreateTokenEndpointContent(new[] { new kvp("client_id", "123"), new kvp("client_secret", "secret123"), new kvp("grant_type", "client_credentials"), new kvp("scope", "scope1 scope2 scope3") });
                    responseMessage = httpClient.PostAsync(tokenEndpointUri, formContent).Result.Content.ReadAsStringAsync().Result;
                    jToken = JToken.Parse(responseMessage);
                    Assert.NotNull(jToken.SelectToken("access_token").Value<string>());
                    Assert.Equal("bearer", jToken.SelectToken("token_type").Value<string>());
                    Assert.NotNull(jToken.SelectToken("expires_in").Value<string>());
                    Assert.Equal("value1", jToken.SelectToken("param1").Value<string>());
                    Assert.Equal("value2", jToken.SelectToken("param2").Value<string>());
                }
                finally
                {
                    //Finally close the client endpoint
                    if (clientEndpoint != null)
                    {
                        clientEndpoint.Dispose();
                    }
                }
            }
        }

        internal void AuthServerHappyPathConfiguration(IAppBuilder app)
        {
            app.UseErrorPage();
            app.UseOAuthAuthorizationServer(new OAuthAuthorizationServerOptions
            {
                AuthorizeEndpointPath = new PathString("/AuthorizeEndpoint"),
                TokenEndpointPath = new PathString("/TokenEndpoint"),
                AllowInsecureHttp = true,
                Provider = new OAuthAuthorizationServerProvider
                {
                    //Authorize endpoint
                    OnValidateClientRedirectUri = (context) =>
                    {
                        context.OwinContext.Set<bool>("OnValidateClientRedirectUri", true);
                        if (context.RedirectUri.Contains("invalid_uri_displayerror"))
                        {
                            context.Options.ApplicationCanDisplayErrors = false;
                            context.Rejected();
                        }
                        else if (context.RedirectUri.Contains("invalid_uri_passonerror"))
                        {
                            context.Options.ApplicationCanDisplayErrors = true;
                            context.SetError("custom.error", "custom.errordescription", "custom.erroruri");
                        }
                        else if (context.ClientId == "123")
                        {
                            context.Validated();
                        }
                        return Task.FromResult(0);
                    },
                    OnValidateAuthorizeRequest = (context) =>
                    {
                        context.OwinContext.Set<bool>("OnValidateAuthorizeRequest", true);

                        if (context.AuthorizeRequest.State == "invalidstate")
                        {
                            context.SetError("state.invalid", "state.invaliddescription", "state.invaliduri");
                        }
                        else if (context.AuthorizeRequest.State == "validstate" || context.AuthorizeRequest.State == null)
                        {
                            context.Validated();
                        }

                        if (context.AuthorizeRequest.Scope == null || context.AuthorizeRequest.Scope.Count != 1 || !context.AuthorizeRequest.Scope.Contains("scope1"))
                        {
                            context.Rejected();
                        }

                        return Task.FromResult(0);
                    },
                    OnAuthorizeEndpoint = (context) =>
                    {
                        var owinContext = context.OwinContext;
                        if (!owinContext.Get<bool>("OnMatchEndpoint") || !owinContext.Get<bool>("OnValidateClientRedirectUri") || !owinContext.Get<bool>("OnValidateAuthorizeRequest"))
                        {
                            //This will make sure no token is sent back
                            owinContext.Response.StatusCode = 400;
                        }
                        else
                        {
                            var claim = new Claim(ClaimTypes.Name, "OnAuthorizeEndpointInvoked");
                            var identity = new ClaimsIdentity(new Claim[] { claim }, context.Options.AuthenticationType);
                            owinContext.Authentication.SignIn(identity);
                            context.RequestCompleted();
                        }
                        return Task.FromResult(0);
                    },

                    //Common
                    OnMatchEndpoint = (context) =>
                    {
                        context.OwinContext.Set<bool>("OnMatchEndpoint", true);
                        return Task.FromResult(0);
                    },

                    //Token endpoint
                    OnValidateClientAuthentication = context =>
                    {
                        string clientId;
                        string clientSecret;
                        if (context.TryGetBasicCredentials(out clientId, out clientSecret) ||
                            context.TryGetFormCredentials(out clientId, out clientSecret))
                        {
                            if (clientId == "123" && clientSecret == "secret123")
                            {
                                context.Validated();
                            }
                        }
                        return Task.FromResult(0);
                    },
                    OnValidateTokenRequest = context =>
                        {
                            context.Validated();
                            return Task.FromResult(0);
                        },
                    OnGrantAuthorizationCode = context =>
                        {
                            context.Validated();
                            return Task.FromResult(0);
                        },
                    OnTokenEndpoint = context =>
                        {
                            context.AdditionalResponseParameters.Add("param1", "value1");
                            context.AdditionalResponseParameters.Add("param2", "value2");
                            context.RequestCompleted();
                            return Task.FromResult(0);
                        },
                    OnGrantResourceOwnerCredentials = context =>
                        {
                            // [SuppressMessage("Microsoft.Security", "CS002:SecretInNextLine", Justification="Unit test dummy credentials.")]
                            if (context.UserName == "user1" && context.Password == "password1")
                            {
                                var scope = context.Scope;
                                if (scope.Count == 3 && scope.Contains("scope1") && scope.Contains("scope2") && scope.Contains("scope3"))
                                {
                                    var claim = new Claim(ClaimTypes.Name, "OnGrantResourceOwnerCredentials");
                                    var identity = new ClaimsIdentity(new Claim[] { claim }, context.Options.AuthenticationType);
                                    context.Validated(identity);
                                }
                            }

                            return Task.FromResult(0);
                        },
                    OnGrantClientCredentials = context =>
                        {
                            var scope = context.Scope;
                            if (scope.Count == 3 && scope.Contains("scope1") && scope.Contains("scope2") && scope.Contains("scope3"))
                            {
                                var claim = new Claim(ClaimTypes.Name, "OnGrantResourceOwnerCredentials");
                                var identity = new ClaimsIdentity(new Claim[] { claim }, context.Options.AuthenticationType);
                                context.Validated(identity);
                            }
                            return Task.FromResult(0);
                        },
                    OnGrantRefreshToken = context =>
                        {
                            //Bug# https://github.com/Katana/katana/issues/592
                            //var scope = context.Scope;
                            //if (scope.Count == 3 && scope.Contains("scope1") && scope.Contains("scope2") && scope.Contains("scope3"))
                            {
                                var claim = new Claim(ClaimTypes.Name, "OnGrantRefreshToken");
                                var identity = new ClaimsIdentity(new Claim[] { claim }, context.Options.AuthenticationType);
                                context.Validated(identity);
                            }
                            return Task.FromResult(0);
                        }
                },
                AuthorizationCodeProvider = new AuthenticationTokenProvider
                {
                    OnCreate = context =>
                    {
                        context.SetToken(Guid.NewGuid().ToString("n") + Guid.NewGuid().ToString("n"));
                        _authenticationCodes[context.Token] = context.SerializeTicket();
                    },
                    OnReceive = context =>
                        {
                            string value;
                            if (_authenticationCodes.TryRemove(context.Token, out value))
                            {
                                context.DeserializeTicket(value);
                            }
                        }
                },
                RefreshTokenProvider = new AuthenticationTokenProvider
                {
                    OnCreate = context =>
                    {
                        context.SetToken(Guid.NewGuid().ToString("n") + Guid.NewGuid().ToString("n"));
                        _refreshTokens[context.Token] = context.SerializeTicket();
                    },
                    OnReceive = context =>
                    {
                        string value;
                        if (_refreshTokens.TryRemove(context.Token, out value))
                        {
                            context.DeserializeTicket(value);
                        }
                    }
                },
            });
            
            //app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());

            //app.Use((context, next) =>
            //{
            //    if(context.Request.User != null && context.Request.User.Identity.IsAuthenticated)
            //    {
            //        return context.Response.WriteAsync("BearerTokenRead");
            //    }

            //    return next.Invoke();
            //});

            app.Run(context =>
            {
                if (context.Get<string>("oauth.Error") == "custom.error" &&
                    context.Get<string>("oauth.ErrorDescription") == "custom.errordescription" &&
                    context.Get<string>("oauth.ErrorUri") == "custom.erroruri")
                {
                    return context.Response.WriteAsync("Custom error page");
                }
                else
                {
                    return context.Response.WriteAsync("FAILURE");
                }
            });
        }
    }
}