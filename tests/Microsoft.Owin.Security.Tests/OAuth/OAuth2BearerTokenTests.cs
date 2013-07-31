// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
    public class OAuth2BearerTokenTests
    {
        [Fact]
        public async Task TokenLocationMayBeChanged()
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
                s.BearerProvider.OnRequestToken = ctx => { ctx.Token = ctx.Request.Query.Get("access_token"); return Task.FromResult(0); };
            });

            OAuth2TestServer.Transaction transaction1 = await server.SendAsync(
                "http://example.com/token",
                authenticateHeader: new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes("alpha:beta"))),
                postBody: "grant_type=client_credentials");

            transaction1.Response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var accessToken = transaction1.ResponseToken.Value<string>("access_token");

            OAuth2TestServer.Transaction transaction2 = await server.SendAsync(
                "http://example.com/me?access_token=" + Uri.EscapeDataString(accessToken));
            transaction2.Response.StatusCode.ShouldBe(HttpStatusCode.OK);
            transaction2.ResponseText.ShouldBe("alpha");
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
