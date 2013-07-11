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
    public class OAuth2AuthorizationClientCredentialsGrantTests
    {
        [Fact]
        public async Task MissingClientCredentialsFails()
        {
            var server = new OAuth2TestServer();

            var transaction1 = await server.SendAsync(
                "http://example.com/token",
                postBody: "grant_type=client_credentials");

            transaction1.Response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            transaction1.ResponseToken.Value<string>("error").ShouldBe("invalid_client");
        }

        [Fact]
        public async Task UnrecognizedClientCredentialsFails()
        {
            var server = new OAuth2TestServer();

            var transaction1 = await server.SendAsync(
                "http://example.com/token",
                authenticateHeader: new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes("bad:data"))),
                postBody: "grant_type=client_credentials");

            transaction1.Response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            transaction1.ResponseToken.Value<string>("error").ShouldBe("invalid_client");
        }

        [Fact]
        public async Task NonPermittedClientFails()
        {
            var server = new OAuth2TestServer();

            var transaction1 = await server.SendAsync(
                "http://example.com/token",
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
            });

            var transaction1 = await server.SendAsync(
                "http://example.com/token",
                authenticateHeader: new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes("alpha:beta"))),
                postBody: "grant_type=client_credentials");

            transaction1.Response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var accessToken = transaction1.ResponseToken.Value<string>("access_token");

            var userName = await GetUserName(server, accessToken);
            userName.ShouldBe("alpha");
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
