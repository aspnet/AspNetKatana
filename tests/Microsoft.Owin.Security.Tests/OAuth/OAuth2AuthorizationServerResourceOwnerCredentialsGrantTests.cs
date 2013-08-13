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
                s.Provider.OnValidateClientAuthentication = ctx =>
                {
                    ctx.Validated();
                    return Task.FromResult(0);
                };
                s.Provider.OnGrantResourceOwnerCredentials = GrantResourceOwnerCredentials("the-username", "the-password");
            });

            OAuth2TestServer.Transaction transaction1 = await server.SendAsync(
                "https://example.com/token",
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
                LookupClient(s.Provider, "one", null, null);
                s.Provider.OnGrantResourceOwnerCredentials = GrantResourceOwnerCredentials("the-username", "the-password");
            });

            OAuth2TestServer.Transaction transaction1 = await server.SendAsync(
                "https://example.com/token",
                postBody: "grant_type=password&username=the-username&password=the-password");

            transaction1.Response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            transaction1.ResponseToken.Value<string>("error").ShouldBe("invalid_client");
        }

        [Fact]
        public async Task ResourceOwnerCanSucceedWithPublicClient()
        {
            var server = new OAuth2TestServer(s =>
            {
                LookupClient(s.Provider, "one", null, "https://example.com/return");
                s.Provider.OnGrantResourceOwnerCredentials = GrantResourceOwnerCredentials("the-username", "the-password");
            });

            LastLookupClientId.ShouldBe(null);

            OAuth2TestServer.Transaction transaction1 = await server.SendAsync(
                "https://example.com/token",
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
                LookupClient(s.Provider, "one", null, "https://example.com/return");
                s.Provider.OnGrantResourceOwnerCredentials = GrantResourceOwnerCredentials("the-username", "the-password");
            });

            LastLookupClientId.ShouldBe(null);

            OAuth2TestServer.Transaction transaction1 = await server.SendAsync(
                "https://example.com/token",
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
                LookupClient(s.Provider, "one", "two", "https://example.com/return");
                s.Provider.OnGrantResourceOwnerCredentials = GrantResourceOwnerCredentials("the-username", "the-password");
            });

            LastLookupClientId.ShouldBe(null);

            OAuth2TestServer.Transaction transaction1 = await server.SendAsync(
                "https://example.com/token",
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
                LookupClient(s.Provider, "one", "two", "https://example.com/return");
                s.Provider.OnGrantResourceOwnerCredentials = GrantResourceOwnerCredentials("the-username", "the-password");
            });

            LastLookupClientId.ShouldBe(null);

            OAuth2TestServer.Transaction transaction1 = await server.SendAsync(
                "https://example.com/token",
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
                LookupClient(s.Provider, "one", null, null);
                s.Provider.OnGrantResourceOwnerCredentials = GrantResourceOwnerCredentials("the-username", "the-password");
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
                "https://example.com/token",
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
                LookupClient(s.Provider, "one", "two", "https://example.com/return");
                s.Provider.OnGrantResourceOwnerCredentials = GrantResourceOwnerCredentials("the-username", "the-password");
            });

            LastLookupClientId.ShouldBe(null);

            OAuth2TestServer.Transaction transaction1 = await server.SendAsync(
                "https://example.com/token",
                postBody: "grant_type=password&username=the-username&password=the-password&client_id=one");

            LastLookupClientId.ShouldBe("one");

            transaction1.Response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            transaction1.ResponseToken.Value<string>("error").ShouldBe("invalid_client");
        }

        private void LookupClient(OAuthAuthorizationServerProvider provider,
            string knownClientId,
            string knownClientSecret,
            string knownRedirectUri)
        {
            provider.OnValidateClientRedirectUri = ctx =>
            {
                LastLookupClientId = ctx.ClientId;
                if (string.Equals(ctx.ClientId, knownClientId, StringComparison.Ordinal))
                {
                    ctx.Validated(knownRedirectUri);
                }
                return Task.FromResult(0);
            };
            provider.OnValidateClientAuthentication = ctx =>
            {
                string clientId;
                string clientSecret;
                if (ctx.TryGetBasicCredentials(out clientId, out clientSecret) ||
                    ctx.TryGetFormCredentials(out clientId, out clientSecret))
                {
                    LastLookupClientId = clientId;
                    if (string.Equals(clientId, knownClientId, StringComparison.Ordinal) &&
                        string.Equals(clientSecret, knownClientSecret, StringComparison.Ordinal))
                    {
                        ctx.Validated(clientId);
                    }
                }
                return Task.FromResult(0);
            };
        }

        private Func<OAuthGrantResourceOwnerCredentialsContext, Task> GrantResourceOwnerCredentials(
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
                    string scope = string.Join(" ", ctx.Scope);
                    if (!String.IsNullOrEmpty(scope))
                    {
                        claims.Add(new Claim("scope", scope));
                    }
                    if (!String.IsNullOrEmpty(ctx.ClientId))
                    {
                        claims.Add(new Claim("client", ctx.ClientId));
                    }
                    ctx.Validated(new ClaimsIdentity(claims, "Bearer"));
                }
                return Task.FromResult(0);
            };
        }

        private async Task<string> GetUserName(OAuth2TestServer server, string accessToken)
        {
            OAuth2TestServer.Transaction transaction = await server.SendAsync("https://example.com/me",
                authenticateHeader: new AuthenticationHeaderValue("Bearer", accessToken));

            transaction.Response.StatusCode.ShouldBe(HttpStatusCode.OK);
            return transaction.ResponseText;
        }
    }
}
