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

namespace Microsoft.Owin.Security.Tests.OAuth
{
    public class OAuth2AuthorizationCustomGrantTests
    {
        [Fact]
        public async Task MissingClientCredentialsFails()
        {
            var server = new OAuth2TestServer(s =>
            {
                s.Provider.OnGrantCustomExtension = ValidateCustomGrant;
            });

            OAuth2TestServer.Transaction transaction1 = await server.SendAsync(
                "http://example.com/token",
                postBody: "grant_type=urn:example:register");

            transaction1.Response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            transaction1.ResponseToken.Value<string>("error").ShouldBe("invalid_client");
        }

        [Fact]
        public async Task UnrecognizedClientCredentialsFails()
        {
            var server = new OAuth2TestServer(s =>
            {
                s.Provider.OnGrantCustomExtension = ValidateCustomGrant;
            });

            OAuth2TestServer.Transaction transaction1 = await server.SendAsync(
                "http://example.com/token",
                authenticateHeader: new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes("bad:data"))),
                postBody: "grant_type=urn:example:register");

            transaction1.Response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            transaction1.ResponseToken.Value<string>("error").ShouldBe("invalid_client");
        }

        [Fact]
        public async Task NonPermittedClientFails()
        {
            var server = new OAuth2TestServer(s =>
            {
                s.Provider.OnGrantCustomExtension = ValidateCustomGrant;
            });

            OAuth2TestServer.Transaction transaction1 = await server.SendAsync(
                "http://example.com/token",
                authenticateHeader: new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes("alpha:beta"))),
                postBody: "grant_type=urn:example:register");

            transaction1.Response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            transaction1.ResponseToken.Value<string>("error").ShouldBe("unauthorized_client");
        }

        [Fact]
        public async Task TokenMayBeIssuedWithCustomGrantType()
        {
            var server = new OAuth2TestServer(s =>
            {
                s.Provider.OnGrantCustomExtension = ValidateCustomGrant;
            });

            OAuth2TestServer.Transaction transaction1 = await server.SendAsync(
                "http://example.com/token",
                authenticateHeader: new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes("alpha2:beta2"))),
                postBody: "grant_type=urn:example:register&alias=one");

            transaction1.Response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var accessToken = transaction1.ResponseToken.Value<string>("access_token");

            string userName = await GetUserName(server, accessToken);
            userName.ShouldBe("one");
        }

        private Task ValidateCustomGrant(OAuthGrantCustomExtensionContext ctx)
        {
            if (ctx.GrantType == "urn:example:register")
            {
                if (ctx.ClientId == "alpha2")
                {
                    var claims = new List<Claim>();
                    claims.Add(new Claim("the-grant-type", ctx.GrantType));
                    claims.Add(new Claim(ClaimsIdentity.DefaultNameClaimType, ctx.Parameters["alias"]));
                    ctx.Validated(new ClaimsIdentity(claims, "Bearer"));
                }
                else
                {
                    ctx.SetError("unauthorized_client");
                }
            }
            return Task.FromResult(0);
        }

        [Fact]
        public async Task CustomGrantTypeMaySendSpecificError()
        {
            var server = new OAuth2TestServer(s =>
            {
                s.Provider.OnGrantCustomExtension = async ctx => ctx.SetError("one", "two", "three");
            });

            OAuth2TestServer.Transaction transaction1 = await server.SendAsync(
                "http://example.com/token",
                authenticateHeader: new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes("alpha:beta"))),
                postBody: "grant_type=urn:example:register&alias=one");

            transaction1.Response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            transaction1.ResponseToken.Value<string>("error").ShouldBe("one");
            transaction1.ResponseToken.Value<string>("error_description").ShouldBe("two");
            transaction1.ResponseToken.Value<string>("error_uri").ShouldBe("three");
        }

        [Fact]
        public async Task TokenGrantsMayCarryAdditionalResponseParameters()
        {
            var server = new OAuth2TestServer(s =>
            {
                s.Provider.OnGrantCustomExtension = ValidateCustomGrant;
                s.Provider.OnTokenEndpoint = async ctx =>
                {
                    ctx.AdditionalResponseParameters["is_registered"] = false;
                    ctx.AdditionalResponseParameters["server_time"] = s.Clock.UtcNow.DateTime;
                    ctx.AdditionalResponseParameters["username"] = ctx.Identity.Name;
                };
            });

            OAuth2TestServer.Transaction transaction1 = await server.SendAsync(
                "http://example.com/token",
                authenticateHeader: new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes("alpha2:beta2"))),
                postBody: "grant_type=urn:example:register&alias=two");

            transaction1.Response.StatusCode.ShouldBe(HttpStatusCode.OK);
            transaction1.ResponseToken.Value<bool>("is_registered").ShouldBe(false);
            transaction1.ResponseToken.Value<DateTime>("server_time").ShouldBe(server.Clock.UtcNow.DateTime);
            transaction1.ResponseToken.Value<string>("username").ShouldBe("two");
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
