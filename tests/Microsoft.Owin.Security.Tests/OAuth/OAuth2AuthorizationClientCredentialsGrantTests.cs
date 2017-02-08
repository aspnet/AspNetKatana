// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Microsoft.Owin.Security.Tests.OAuth
{
    public class OAuth2AuthorizationClientCredentialsGrantTests
    {
        [Fact]
        public async Task MissingClientCredentialsFails()
        {
            var server = new OAuth2TestServer();

            OAuth2TestServer.Transaction transaction1 = await server.SendAsync(
                "https://example.com/token",
                postBody: "grant_type=client_credentials");

            transaction1.Response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            transaction1.ResponseToken.Value<string>("error").ShouldBe("invalid_client");
        }

        [Fact]
        public async Task UnrecognizedClientCredentialsFails()
        {
            var server = new OAuth2TestServer();

            OAuth2TestServer.Transaction transaction1 = await server.SendAsync(
                "https://example.com/token",
                authenticateHeader: new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes("bad:data"))),
                postBody: "grant_type=client_credentials");

            transaction1.Response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            transaction1.ResponseToken.Value<string>("error").ShouldBe("invalid_client");
        }

        [Fact]
        public async Task BadlyFormattedClientCredentialsFails()
        {
            var server = new OAuth2TestServer();

            OAuth2TestServer.Transaction transaction1 = await server.SendAsync(
                "https://example.com/token",
                authenticateHeader: new AuthenticationHeaderValue("Basic", "InvalidBase64"),
                postBody: "grant_type=client_credentials");

            transaction1.Response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            transaction1.ResponseToken.Value<string>("error").ShouldBe("invalid_client");
        }

        [Fact]
        public async Task BadUtf8ClientCredentialsFails()
        {
            var server = new OAuth2TestServer();

            OAuth2TestServer.Transaction transaction1 = await server.SendAsync(
                "https://example.com/token",
                authenticateHeader: new AuthenticationHeaderValue("Basic", Convert.ToBase64String(new byte[] { 0x8F, 0x90 })),
                postBody: "grant_type=client_credentials");

            transaction1.Response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            transaction1.ResponseToken.Value<string>("error").ShouldBe("invalid_client");
        }

        [Fact]
        public async Task NonPermittedClientFails()
        {
            var server = new OAuth2TestServer();

            OAuth2TestServer.Transaction transaction1 = await server.SendAsync(
                "https://example.com/token",
                authenticateHeader: new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes("alpha:beta"))),
                postBody: "grant_type=client_credentials");

            transaction1.Response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            transaction1.ResponseToken.Value<string>("error").ShouldBe("unauthorized_client");
        }

        [Fact]
        public async Task TokenMayBeIssuedToClient()
        {
            var server = new OAuth2TestServer(s =>
            {
                s.Provider.OnGrantClientCredentials = ctx =>
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimsIdentity.DefaultNameClaimType, ctx.ClientId),
                    };
                    string scope = string.Join(" ", ctx.Scope);
                    if (!string.IsNullOrEmpty(scope))
                    {
                        claims.Add(new Claim("scope", scope));
                    }
                    ctx.Validated(new ClaimsIdentity(claims, "Bearer"));
                    return Task.FromResult(0);
                };
            });

            OAuth2TestServer.Transaction transaction1 = await server.SendAsync(
                "https://example.com/token",
                authenticateHeader: new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes("alpha:beta"))),
                postBody: "grant_type=client_credentials");

            transaction1.Response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var accessToken = transaction1.ResponseToken.Value<string>("access_token");

            string userName = await GetUserName(server, accessToken);
            userName.ShouldBe("alpha");
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
