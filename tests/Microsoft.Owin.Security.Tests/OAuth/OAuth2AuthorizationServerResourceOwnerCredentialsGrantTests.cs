// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.Security.OAuth;
using Shouldly;
using Xunit;
using Xunit.Extensions;

namespace Microsoft.Owin.Security.Tests.OAuth
{
    public class OAuth2AuthorizationServerResourceOwnerCredentialsGrantTests
    {
        protected string LastLookupClientId { get; set; }

        [Fact]
        public async Task ResourceOwnerCanSucceedWithoutClientId()
        {
            var server = new OAuth2TestServer(s =>
            {
                s.Provider.OnLookupClient = ctx =>
                {
                    if (String.IsNullOrEmpty(ctx.ClientId))
                    {
                        // allow client_id-less request
                        ctx.ClientFound(null, null);
                    }
                    return Task.FromResult(0);
                };
                s.Provider.OnValidateResourceOwnerCredentials = ValidateResourceOwnerCredentials("the-username", "the-password");
            });

            OAuth2TestServer.Transaction transaction1 = await server.SendAsync(
                "http://example.com/token",
                postBody: "grant_type=password&username=the-username&password=the-password");

            transaction1.Response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var accessToken = transaction1.ResponseToken.Value<string>("access_token");

            string userName = await GetUserName(server, accessToken);
            userName.ShouldBe("the-username");
        }

        [Fact]
        public async Task ResourceOwnerFailsWithoutClientWhenNotExplicitlyEnabled()
        {
            var server = new OAuth2TestServer(s =>
            {
                s.Provider.OnLookupClient = LookupClient("one", null, null);
                s.Provider.OnValidateResourceOwnerCredentials = ValidateResourceOwnerCredentials("the-username", "the-password");
            });

            OAuth2TestServer.Transaction transaction1 = await server.SendAsync(
                "http://example.com/token",
                postBody: "grant_type=password&username=the-username&password=the-password");

            transaction1.Response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            transaction1.ResponseToken.Value<string>("error").ShouldBe("invalid_client");
        }

        [Fact]
        public async Task ResourceOwnerCanSucceedWithPublicClient()
        {
            var server = new OAuth2TestServer(s =>
            {
                s.Provider.OnLookupClient = LookupClient("one", null, "http://example.com/return");
                s.Provider.OnValidateResourceOwnerCredentials = ValidateResourceOwnerCredentials("the-username", "the-password");
            });

            LastLookupClientId.ShouldBe(null);

            OAuth2TestServer.Transaction transaction1 = await server.SendAsync(
                "http://example.com/token",
                postBody: "grant_type=password&username=the-username&password=the-password&client_id=one");

            LastLookupClientId.ShouldBe("one");

            transaction1.Response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var accessToken = transaction1.ResponseToken.Value<string>("access_token");

            string userName = await GetUserName(server, accessToken);
            userName.ShouldBe("the-username");
        }

        [Fact]
        public async Task ResourceOwnerFailsWhenPublicClientProvidesCredentials()
        {
            var server = new OAuth2TestServer(s =>
            {
                s.Provider.OnLookupClient = LookupClient("one", null, "http://example.com/return");
                s.Provider.OnValidateResourceOwnerCredentials = ValidateResourceOwnerCredentials("the-username", "the-password");
            });

            LastLookupClientId.ShouldBe(null);

            OAuth2TestServer.Transaction transaction1 = await server.SendAsync(
                "http://example.com/token",
                authenticateHeader: new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes("one:two"))),
                postBody: "grant_type=password&username=the-username&password=the-password&client_id=one");

            LastLookupClientId.ShouldBe("one");

            transaction1.Response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            transaction1.ResponseToken.Value<string>("error").ShouldBe("invalid_client");
        }

        [Fact]
        public async Task ResourceOwnerCanSucceedWithConfidentialClient()
        {
            var server = new OAuth2TestServer(s =>
            {
                s.Provider.OnLookupClient = LookupClient("one", "two", "http://example.com/return");
                s.Provider.OnValidateResourceOwnerCredentials = ValidateResourceOwnerCredentials("the-username", "the-password");
            });

            LastLookupClientId.ShouldBe(null);

            OAuth2TestServer.Transaction transaction1 = await server.SendAsync(
                "http://example.com/token",
                authenticateHeader: new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes("one:two"))),
                postBody: "grant_type=password&username=the-username&password=the-password");

            LastLookupClientId.ShouldBe("one");

            transaction1.Response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var accessToken = transaction1.ResponseToken.Value<string>("access_token");

            string userName = await GetUserName(server, accessToken);
            userName.ShouldBe("the-username");
        }

        [Fact]
        public async Task ResourceOwnerFailsWhenConfidentialClientMissingCredentials()
        {
            var server = new OAuth2TestServer(s =>
            {
                s.Provider.OnLookupClient = LookupClient("one", "two", "http://example.com/return");
                s.Provider.OnValidateResourceOwnerCredentials = ValidateResourceOwnerCredentials("the-username", "the-password");
            });

            LastLookupClientId.ShouldBe(null);

            OAuth2TestServer.Transaction transaction1 = await server.SendAsync(
                "http://example.com/token",
                postBody: "grant_type=password&username=the-username&password=the-password&client_id=one");

            LastLookupClientId.ShouldBe("one");

            transaction1.Response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            transaction1.ResponseToken.Value<string>("error").ShouldBe("invalid_client");
        }

        [Theory]
        [InlineData("the-username", null)]
        [InlineData("the-username", "")]
        [InlineData("the-username", "wrong-password")]
        [InlineData(null, "the-password")]
        [InlineData("", "the-password")]
        [InlineData("wrong-username", "the-password")]
        public async Task FailsWhenWrongOwnerCredentialsProvided(string username, string password)
        {
            var server = new OAuth2TestServer(s =>
            {
                s.Provider.OnLookupClient = LookupClient("one", null, null);
                s.Provider.OnValidateResourceOwnerCredentials = ValidateResourceOwnerCredentials("the-username", "the-password");
            });

            LastLookupClientId.ShouldBe(null);

            string body = "grant_type=password&client_id=one";
            if (username != null)
            {
                body += "&username=" + Uri.EscapeDataString(username);
            }
            if (password != null)
            {
                body += "&password=" + Uri.EscapeDataString(password);
            }
            OAuth2TestServer.Transaction transaction1 = await server.SendAsync(
                "http://example.com/token",
                postBody: body);

            LastLookupClientId.ShouldBe("one");

            transaction1.Response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            transaction1.ResponseToken.Value<string>("error").ShouldBe("invalid_grant");
        }

        [Fact]
        public async Task FailsWhenWrongPasswordProvided()
        {
            var server = new OAuth2TestServer(s =>
            {
                s.Provider.OnLookupClient = LookupClient("one", "two", "http://example.com/return");
                s.Provider.OnValidateResourceOwnerCredentials = ValidateResourceOwnerCredentials("the-username", "the-password");
            });

            LastLookupClientId.ShouldBe(null);

            OAuth2TestServer.Transaction transaction1 = await server.SendAsync(
                "http://example.com/token",
                postBody: "grant_type=password&username=the-username&password=the-password&client_id=one");

            LastLookupClientId.ShouldBe("one");

            transaction1.Response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            transaction1.ResponseToken.Value<string>("error").ShouldBe("invalid_client");
        }

        private Func<OAuthLookupClientContext, Task> LookupClient(
            string clientId,
            string clientSecret,
            string redirectUri)
        {
            return ctx =>
            {
                LastLookupClientId = clientId;
                if (String.Equals(clientId, ctx.ClientId, StringComparison.Ordinal))
                {
                    // allow client_id-less request
                    ctx.ClientFound(clientSecret, redirectUri);
                }
                return Task.FromResult(0);
            };
        }

        private Func<OAuthValidateResourceOwnerCredentialsContext, Task> ValidateResourceOwnerCredentials(
            string userName,
            string password)
        {
            return ctx =>
            {
                if (ctx.UserName == userName && ctx.Password == password)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimsIdentity.DefaultNameClaimType, ctx.UserName)
                    };
                    if (!String.IsNullOrEmpty(ctx.Scope))
                    {
                        claims.Add(new Claim("scope", ctx.Scope));
                    }
                    if (!String.IsNullOrEmpty(ctx.ClientId))
                    {
                        claims.Add(new Claim("client", ctx.ClientId));
                    }
                    ctx.Validated(new ClaimsIdentity(claims, "Bearer"), new AuthenticationExtra());
                }
                return Task.FromResult(0);
            };
        }

        private async Task<string> GetUserName(OAuth2TestServer server, string accessToken)
        {
            OAuth2TestServer.Transaction transaction = await server.SendAsync("http://example.com/me",
                authenticateHeader: new AuthenticationHeaderValue("Bearer", accessToken));

            transaction.Response.StatusCode.ShouldBe(HttpStatusCode.OK);
            return transaction.ResponseText;
        }
    }
}
