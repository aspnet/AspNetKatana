// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.Security.Infrastructure;
using Newtonsoft.Json.Linq;
using Shouldly;
using Xunit;

namespace Microsoft.Owin.Security.Tests
{
    public class OAuth2AuthorizationServerTests
    {
        [Fact]
        public async Task MissingClientIdDoesNotRedirect()
        {
            var server = new OAuth2TestServer();

            var transaction = await server.SendAsync("http://example.com/authorize");

            transaction.Response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            transaction.ResponseText.ShouldContain("invalid_request");
        }

        [Fact]
        public async Task IncorrectRedirectUriDoesNotRedirect()
        {
            var server = new OAuth2TestServer();

            var transaction = await server.SendAsync("http://example.com/authorize?client_id=alpha&redirect_uri=" + Uri.EscapeDataString("http://wrongplace.com/"));

            transaction.Response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            transaction.ResponseText.ShouldContain("invalid_request");
        }

        [Fact]
        public async Task MissingResponseTypeRedirectsWithErrorMessage()
        {
            var server = new OAuth2TestServer();

            var transaction = await server.SendAsync("http://example.com/authorize?client_id=alpha");

            transaction.Response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
            transaction.Response.Headers.Location.Query.ShouldContain("error=invalid_request");
        }

        [Fact]
        public async Task UnsupportedResponseTypeRedirectsWithErrorMessage()
        {
            var server = new OAuth2TestServer();

            var transaction = await server.SendAsync("http://example.com/authorize?client_id=alpha&response_type=delta");

            transaction.Response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
            transaction.Response.Headers.Location.Query.ShouldContain("error=unsupported_response_type");
        }

        [Fact]
        public async Task AuthorizeRequestMayPassThroughToApplicationRequestHandler()
        {
            var server = new OAuth2TestServer
            {
                OnAuthorizeEndpoint = async ctx =>
                {
                    ctx.Response.ContentType = "text/plain";
                    using (var writer = new StreamWriter(ctx.Response.Body, Encoding.UTF8, 4096, leaveOpen: true))
                    {
                        await writer.WriteAsync("Responding");
                    }
                }
            };

            var transaction = await server.SendAsync("http://example.com/authorize?client_id=alpha&response_type=code");
            transaction.Response.StatusCode.ShouldBe(HttpStatusCode.OK);
            transaction.ResponseText.ShouldBe("Responding");
        }

        [Fact]
        public async Task CallingSignInWillRedirectWithAuthorizationCode()
        {
            var server = new OAuth2TestServer
            {
                OnAuthorizeEndpoint = ctx =>
                {
                    ctx.Authentication.SignIn(new AuthenticationExtra(), CreateIdentity("epsilon"));
                    return Task.FromResult<object>(null);
                }
            };

            var transaction = await server.SendAsync("http://example.com/authorize?client_id=alpha&response_type=code");
            transaction.Response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
            transaction.Response.Headers.Location.Query.ShouldContain("code=");
        }

        [Fact]
        public async Task NonTwoHundredDoesNotGetChanged()
        {
            var server = new OAuth2TestServer
            {
                OnAuthorizeEndpoint = ctx =>
                {
                    ctx.Response.StatusCode = 404;
                    ctx.Authentication.SignIn(
                        new AuthenticationExtra(),
                        CreateIdentity("epsilon"));
                    return Task.FromResult<object>(null);
                }
            };

            var transaction = await server.SendAsync("http://example.com/authorize?client_id=alpha&response_type=code");
            transaction.Response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
            transaction.Response.Headers.Location.ShouldBe(null);
        }

        [Fact]
        public async Task CodeCanBeExchangedForToken()
        {
            var server = new OAuth2TestServer
            {
                OnAuthorizeEndpoint = SignInEpsilon
            };

            var transaction = await server.SendAsync("http://example.com/authorize?client_id=alpha&response_type=code");

            var query = transaction.ParseRedirectQueryString();

            var transaction2 = await server.SendAsync("http://example.com/token", postBody:
                "grant_type=authorization_code&code=" + query["code"] + "&client_id=alpha");

            transaction2.ResponseToken["access_token"].Value<string>().ShouldNotBe(null);
            transaction2.ResponseToken["token_type"].Value<string>().ShouldBe("bearer");
        }

        private Task SignInEpsilon(IOwinContext ctx)
        {
            ctx.Authentication.SignIn(new AuthenticationExtra(), CreateIdentity("epsilon"));
            return Task.FromResult<object>(null);
        }

        [Fact]
        public async Task CodeExpiresAfterGivenTimespan()
        {
            var server = new OAuth2TestServer(s =>
            {
                s.Options.AuthenticationCodeExpireTimeSpan = TimeSpan.FromMinutes(8);
                s.OnAuthorizeEndpoint = SignInEpsilon;
            });

            var transaction = await server.SendAsync("http://example.com/authorize?client_id=alpha&response_type=code");

            var query = transaction.ParseRedirectQueryString();

            server.Clock.Add(TimeSpan.FromMinutes(10));

            var transaction2 = await server.SendAsync("http://example.com/token",
                authenticateHeader: new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes("alpha:beta"))),
                postBody: "grant_type=authorization_code&code=" + query["code"] + "&client_id=alpha");

            transaction2.ResponseToken["error"].Value<string>().ShouldBe("invalid_grant");
        }

        [Fact]
        public async Task TokenTellsYouHowManySecondsItWillExpireIn()
        {
            var server = new OAuth2TestServer(s =>
            {
                s.Options.AuthenticationCodeExpireTimeSpan = TimeSpan.FromMinutes(8);
                s.Options.AccessTokenExpireTimeSpan = TimeSpan.FromSeconds(655321);
                s.OnAuthorizeEndpoint = SignInEpsilon;
            });

            var transaction = await server.SendAsync("http://example.com/authorize?client_id=alpha&response_type=code");

            var query = transaction.ParseRedirectQueryString();

            var transaction2 = await server.SendAsync("http://example.com/token",
                authenticateHeader: new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes("alpha:beta"))),
                postBody: "grant_type=authorization_code&code=" + query["code"] + "&client_id=alpha");

            transaction2.ResponseToken["access_token"].Value<string>().ShouldNotBe(null);
            transaction2.ResponseToken["token_type"].Value<string>().ShouldBe("bearer");
            transaction2.ResponseToken["expires_in"].Value<long>().ShouldBe(655321);
        }

        [Fact]
        public async Task CodeCanBeUsedOnlyOneTime()
        {
            var server = new OAuth2TestServer(s =>
            {
                s.Options.AuthenticationCodeExpireTimeSpan = TimeSpan.FromMinutes(8);
                s.Options.AccessTokenExpireTimeSpan = TimeSpan.FromSeconds(655321);
                s.OnAuthorizeEndpoint = SignInEpsilon;
            });

            var transaction = await server.SendAsync("http://example.com/authorize?client_id=alpha&response_type=code");

            var query = transaction.ParseRedirectQueryString();

            var transaction2 = await server.SendAsync("http://example.com/token",
                authenticateHeader: new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes("alpha:beta"))),
                postBody: "grant_type=authorization_code&code=" + query["code"] + "&client_id=alpha");

            transaction2.ResponseToken["access_token"].Value<string>().ShouldNotBe(null);
            transaction2.ResponseToken["token_type"].Value<string>().ShouldBe("bearer");
            transaction2.ResponseToken["expires_in"].Value<long>().ShouldBe(655321);

            var transaction3 = await server.SendAsync("http://example.com/token",
                authenticateHeader: new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes("alpha:beta"))),
                postBody: "grant_type=authorization_code&code=" + query["code"] + "&client_id=alpha");

            transaction3.ResponseToken["error"].Value<string>().ShouldBe("invalid_grant");
        }

        [Fact]
        public async Task RefreshTokenMayBeProvided()
        {
            var server = new OAuth2TestServer(s =>
            {
                s.Options.RefreshTokenProvider = new AuthenticationTokenProvider
                {
                    OnCreate = ctx => ctx.SetToken(ctx.SerializeTicket()),
                    OnReceive = ctx => ctx.DeserializeTicket(ctx.Token),
                };
                s.OnAuthorizeEndpoint = SignInEpsilon;
            });

            var transaction = await server.SendAsync("http://example.com/authorize?client_id=alpha&response_type=code");

            var query = transaction.ParseRedirectQueryString();

            var transaction2 = await server.SendAsync("http://example.com/token",
                authenticateHeader: new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes("alpha:beta"))),
                postBody: "grant_type=authorization_code&code=" + query["code"] + "&client_id=alpha");

            transaction2.ResponseToken["access_token"].Value<string>().ShouldNotBe(null);
            transaction2.ResponseToken["refresh_token"].Value<string>().ShouldNotBe(null);
        }

        [Fact]
        public async Task RefreshTokenMayBeUsedToGetNewAccessToken()
        {
            var server = new OAuth2TestServer(s =>
            {
                s.Options.RefreshTokenProvider = new AuthenticationTokenProvider
                {
                    OnCreate = ctx => ctx.SetToken(ctx.SerializeTicket()),
                    OnReceive = ctx => ctx.DeserializeTicket(ctx.Token),
                };
                s.OnAuthorizeEndpoint = SignInEpsilon;
            });

            var transaction = await server.SendAsync("http://example.com/authorize?client_id=alpha&response_type=code");

            var query = transaction.ParseRedirectQueryString();

            var transaction2 = await server.SendAsync("http://example.com/token",
                authenticateHeader: new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes("alpha:beta"))),
                postBody: "grant_type=authorization_code&code=" + query["code"] + "&client_id=alpha");

            var accessToken = transaction2.ResponseToken["access_token"].Value<string>();
            var refreshToken = transaction2.ResponseToken["refresh_token"].Value<string>();
            accessToken.ShouldNotBe(null);
            refreshToken.ShouldNotBe(null);

            var transaction3 = await server.SendAsync("http://example.com/token",
                authenticateHeader: new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes("alpha:beta"))),
                postBody: "grant_type=refresh_token&refresh_token=" + refreshToken);

            var accessToken2 = transaction3.ResponseToken["access_token"].Value<string>();
            var refreshToken2 = transaction3.ResponseToken["refresh_token"].Value<string>();
            accessToken2.ShouldNotBe(null);
            refreshToken2.ShouldNotBe(null);
            accessToken2.ShouldNotBe(accessToken);
            refreshToken2.ShouldNotBe(refreshToken);
        }

        [Fact]
        public async Task AccessTokenWillExpire()
        {
            var server = new OAuth2TestServer(s =>
            {
                s.Options.AuthenticationCodeExpireTimeSpan = TimeSpan.FromMinutes(5);
                s.Options.AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(60);
                s.OnAuthorizeEndpoint = SignInEpsilon;
            });

            var transaction = await server.SendAsync("http://example.com/authorize?client_id=alpha&response_type=code");

            var query = transaction.ParseRedirectQueryString();

            var transaction2 = await server.SendAsync("http://example.com/token",
                authenticateHeader: new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes("alpha:beta"))),
                postBody: "grant_type=authorization_code&code=" + query["code"] + "&client_id=alpha");

            var accessToken = transaction2.ResponseToken["access_token"].Value<string>();

            var transaction3 = await server.SendAsync("http://example.com/me",
                authenticateHeader: new AuthenticationHeaderValue("Bearer", accessToken));

            transaction3.Response.StatusCode.ShouldBe(HttpStatusCode.OK);
            transaction3.ResponseText.ShouldBe("epsilon");

            server.Clock.Add(TimeSpan.FromMinutes(45));

            var transaction4 = await server.SendAsync("http://example.com/me",
                authenticateHeader: new AuthenticationHeaderValue("Bearer", accessToken));

            transaction4.Response.StatusCode.ShouldBe(HttpStatusCode.OK);
            transaction3.ResponseText.ShouldBe("epsilon");

            server.Clock.Add(TimeSpan.FromMinutes(20));

            var transaction5 = await server.SendAsync("http://example.com/me",
                authenticateHeader: new AuthenticationHeaderValue("Bearer", accessToken));

            transaction5.Response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }

        // [Fact]
        public async Task CodeFlowClientIdMustMatch()
        {
            var server = new OAuth2TestServer(s =>
            {
                s.Options.AuthenticationCodeExpireTimeSpan = TimeSpan.FromMinutes(5);
                s.Options.AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(60);
                s.OnAuthorizeEndpoint = SignInEpsilon;
            });

            var transaction = await server.SendAsync("http://example.com/authorize?client_id=alpha1&response_type=code");

            var query = transaction.ParseRedirectQueryString();

            var transaction2 = await server.SendAsync("http://example.com/token",
                authenticateHeader: new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes("alpha2:beta2"))),
                postBody: "grant_type=authorization_code&code=" + query["code"] + "&client_id=alpha2");

            transaction2.Response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            transaction2.ResponseToken["error"].Value<string>().ShouldBe("invalid_grant");
        }

        [Fact]
        public async Task CodeFlowRedirectUriMustBeRepeatedIfOriginallyProvided()
        {
            var server = new OAuth2TestServer(s =>
            {
                s.Options.AuthenticationCodeExpireTimeSpan = TimeSpan.FromMinutes(5);
                s.Options.AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(60);
                s.OnAuthorizeEndpoint = SignInEpsilon;
            });

            var transaction = await server.SendAsync("http://example.com/authorize?client_id=alpha&response_type=code&redirect_uri=" + Uri.EscapeDataString("http://gamma.com/return"));

            var query = transaction.ParseRedirectQueryString();

            var transaction2 = await server.SendAsync("http://example.com/token",
                authenticateHeader: new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes("alpha:beta"))),
                postBody: "grant_type=authorization_code&code=" + query["code"] + "&client_id=alpha");

            transaction2.Response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            transaction2.ResponseToken["error"].Value<string>().ShouldBe("invalid_grant");
        }
        
        // [Fact]
        public async Task CodeFlowRedirectUriMustBeCorrectIfOriginallyProvided()
        {
            var server = new OAuth2TestServer(s =>
            {
                s.Options.AuthenticationCodeExpireTimeSpan = TimeSpan.FromMinutes(5);
                s.Options.AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(60);
                s.OnAuthorizeEndpoint = SignInEpsilon;
            });

            var transaction = await server.SendAsync("http://example.com/authorize?client_id=alpha&response_type=code&redirect_uri=" + Uri.EscapeDataString("http://gamma.com/return"));

            var query = transaction.ParseRedirectQueryString();

            var transaction2 = await server.SendAsync("http://example.com/token",
                authenticateHeader: new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes("alpha:beta"))),
                postBody: "grant_type=authorization_code&code=" + query["code"] + "&client_id=alpha&redirect_uri=" + Uri.EscapeDataString("http://gamma2.com/return"));

            transaction2.Response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            transaction2.ResponseToken["error"].Value<string>().ShouldBe("invalid_grant");
        }

        // [Fact]
        public async Task CodeFlowRedirectUriMustMatch()
        {
            var server = new OAuth2TestServer(s =>
            {
                s.Options.AuthenticationCodeExpireTimeSpan = TimeSpan.FromMinutes(5);
                s.Options.AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(60);
                s.OnAuthorizeEndpoint = SignInEpsilon;
            });

            var transaction = await server.SendAsync("http://example.com/authorize?client_id=alpha2&response_type=code");

            var query = transaction.ParseRedirectQueryString();

            var transaction2 = await server.SendAsync("http://example.com/token",
                authenticateHeader: new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes("alpha2:beta2"))),
                postBody: "grant_type=authorization_code&code=" + query["code"] + "&client_id=alpha2");

            transaction2.Response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            transaction2.ResponseToken["error"].Value<string>().ShouldBe("invalid_grant");
        }

        private static ClaimsIdentity CreateIdentity(string name, params string[] scopes)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, name)
            };
            foreach (var scope in scopes)
            {
                claims.Add(new Claim("scope", scope));
            }
            return new ClaimsIdentity(
                claims,
                "Bearer");
        }
    }
}
