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
    public class OAuth2AuthorizationServerImplicitGrantTests
    {
        [Fact]
        public async Task MissingClientIdDoesNotRedirect()
        {
            var server = new OAuth2TestServer();

            var transaction1 = await server.SendAsync("http://example.com/authorize?response_type=token");

            transaction1.Response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            transaction1.ResponseText.ShouldContain("invalid_request");
        }

        [Fact]
        public async Task BadClientIdDoesNotRedirect()
        {
            var server = new OAuth2TestServer();

            var transaction1 = await server.SendAsync("http://example.com/authorize?response_type=token&client_id=wrong");

            transaction1.Response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            transaction1.ResponseText.ShouldContain("invalid_request");
        }

        [Fact]
        public async Task IncorrectRedirectUriDoesNotRedirect()
        {
            var server = new OAuth2TestServer();

            var transaction1 = await server.SendAsync("http://example.com/authorize?response_type=token&client_id=alpha&redirect_uri=" + Uri.EscapeDataString("http://gamma2.com/return"));

            transaction1.Response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            transaction1.ResponseText.ShouldContain("invalid_request");
        }

        [Fact]
        public async Task ShouldRedirectWithParametersInFragment()
        {
            var server = new OAuth2TestServer(s =>
            {
                s.OnAuthorizeEndpoint = SignInEpsilon;
            });

            var transaction1 = await server.SendAsync("http://example.com/authorize?response_type=token&client_id=alpha&redirect_uri=" + Uri.EscapeDataString("http://gamma.com/return"));

            NameValueCollection fragment = transaction1.ParseRedirectFragment();
            fragment.Get("access_token").ShouldNotBe(null);
            fragment.Get("expires_in").ShouldNotBe(null);
        }

        [Fact]
        public async Task StateShouldBePassedBack()
        {
            var server = new OAuth2TestServer(s =>
            {
                s.OnAuthorizeEndpoint = SignInEpsilon;
            });

            var transaction1 = await server.SendAsync("http://example.com/authorize?response_type=token&client_id=alpha&state=123");

            NameValueCollection fragment = transaction1.ParseRedirectFragment();
            fragment.Get("access_token").ShouldNotBe(null);
            fragment.Get("state").ShouldBe("123");
        }


        [Fact]
        public async Task AccessTokenMayBeUsed()
        {
            var server = new OAuth2TestServer(s =>
            {
                s.OnAuthorizeEndpoint = SignInEpsilon;
            });

            var transaction1 = await server.SendAsync("http://example.com/authorize?response_type=token&client_id=alpha&redirect_uri=" + Uri.EscapeDataString("http://gamma.com/return"));

            NameValueCollection fragment = transaction1.ParseRedirectFragment();
            var accessToken = fragment.Get("access_token");

            var transaction2 = await server.SendAsync("http://example.com/me",
                authenticateHeader: new AuthenticationHeaderValue("Bearer", accessToken));

            transaction2.Response.StatusCode.ShouldBe(HttpStatusCode.OK);
            transaction2.ResponseText.ShouldBe("epsilon");
        }

        [Fact]
        public async Task UnrecognizedParametersAreIgnored()
        {
            var server = new OAuth2TestServer(s =>
            {
                s.OnAuthorizeEndpoint = SignInEpsilon;
            });

            var transaction1 = await server.SendAsync("http://example.com/authorize?alpha=beta&response_type=token&client_id=alpha&redirect_uri=" + Uri.EscapeDataString("http://gamma.com/return"));

            NameValueCollection fragment = transaction1.ParseRedirectFragment();

            var userName = await GetUserName(server, fragment.Get("access_token"));
            userName.ShouldBe("epsilon");
        }

        private async Task SignInEpsilon(IOwinContext ctx)
        {
            ctx.Authentication.SignIn(new AuthenticationExtra(), CreateIdentity("epsilon"));
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

        private async Task<string> GetUserName(OAuth2TestServer server, string accessToken)
        {
            var transaction = await server.SendAsync("http://example.com/me",
                authenticateHeader: new AuthenticationHeaderValue("Bearer", accessToken));

            transaction.Response.StatusCode.ShouldBe(HttpStatusCode.OK);
            return transaction.ResponseText;
        }
    }
}
