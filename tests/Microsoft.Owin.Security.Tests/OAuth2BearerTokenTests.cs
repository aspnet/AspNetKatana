using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Microsoft.Owin.Security.Tests
{
    public class OAuth2BearerTokenTests
    {
        [Fact]
        public async Task TokenLocationMayBeChanged()
        {
            var server = new OAuth2TestServer(s =>
            {
                s.Provider.OnValidateClientCredentials = async ctx =>
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimsIdentity.DefaultNameClaimType, ctx.ClientId),
                    };
                    if (!string.IsNullOrEmpty(ctx.Scope))
                    {
                        claims.Add(new Claim("scope", ctx.Scope));
                    }
                    ctx.Validated(new ClaimsIdentity(claims, "Bearer"), new Dictionary<string, string>());
                };
                s.BearerProvider.OnRequestToken = async ctx =>
                {
                    ctx.Token = ctx.Request.Query.Get("access_token");
                };
            });

            var transaction1 = await server.SendAsync(
                "http://example.com/token",
                authenticateHeader: new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes("alpha:beta"))),
                postBody: "grant_type=client_credentials");

            transaction1.Response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var accessToken = transaction1.ResponseToken.Value<string>("access_token");

            var transaction2 = await server.SendAsync(
                "http://example.com/me?access_token=" + Uri.EscapeDataString(accessToken));
            transaction2.Response.StatusCode.ShouldBe(HttpStatusCode.OK);
            transaction2.ResponseText.ShouldBe("alpha");
        }

        private async Task<string> GetUserName(OAuth2TestServer server, string accessToken)
        {
            var transaction = await server.SendAsync("http://example.com/me",
                authenticateHeader: new AuthenticationHeaderValue("Bearer", accessToken));

            transaction.Response.StatusCode.ShouldBe(HttpStatusCode.OK);
            return transaction.ResponseText;
        }
    }
}
