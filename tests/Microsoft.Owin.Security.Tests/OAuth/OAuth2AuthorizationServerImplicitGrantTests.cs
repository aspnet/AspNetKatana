// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Microsoft.Owin.Security.Tests.OAuth
{
    public class OAuth2AuthorizationServerImplicitGrantTests
    {
        [Fact]
        public async Task MissingClientIdDoesNotRedirect()
        {
            var server = new OAuth2TestServer();

            OAuth2TestServer.Transaction transaction1 = await server.SendAsync("http://example.com/authorize?response_type=token");

            transaction1.Response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            transaction1.ResponseText.ShouldContain("invalid_request");
        }

        [Fact]
        public async Task BadClientIdDoesNotRedirect()
        {
            var server = new OAuth2TestServer();

            OAuth2TestServer.Transaction transaction1 = await server.SendAsync("http://example.com/authorize?response_type=token&client_id=wrong");

            transaction1.Response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            transaction1.ResponseText.ShouldContain("invalid_request");
        }

        [Fact]
        public async Task IncorrectRedirectUriDoesNotRedirect()
        {
            var server = new OAuth2TestServer();

            OAuth2TestServer.Transaction transaction1 = await server.SendAsync("http://example.com/authorize?response_type=token&client_id=alpha&redirect_uri=" + Uri.EscapeDataString("http://gamma2.com/return"));

            transaction1.Response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            transaction1.ResponseText.ShouldContain("invalid_request");
        }

        [Fact]
        public async Task ShouldRedirectWithParametersInFragment()
        {
            var server = new OAuth2TestServer(s => { s.OnAuthorizeEndpoint = SignInEpsilon; });

            OAuth2TestServer.Transaction transaction1 = await server.SendAsync("http://example.com/authorize?response_type=token&client_id=alpha&redirect_uri=" + Uri.EscapeDataString("http://gamma.com/return"));

            NameValueCollection fragment = transaction1.ParseRedirectFragment();
            fragment.Get("access_token").ShouldNotBe(null);
            fragment.Get("expires_in").ShouldNotBe(null);
        }

        [Fact]
        public async Task StateShouldBePassedBack()
        {
            var server = new OAuth2TestServer(s => { s.OnAuthorizeEndpoint = SignInEpsilon; });

            OAuth2TestServer.Transaction transaction1 = await server.SendAsync("http://example.com/authorize?response_type=token&client_id=alpha&state=123");

            NameValueCollection fragment = transaction1.ParseRedirectFragment();
            fragment.Get("access_token").ShouldNotBe(null);
            fragment.Get("state").ShouldBe("123");
        }

        [Fact]
        public async Task AccessTokenMayBeUsed()
        {
            var server = new OAuth2TestServer(s => { s.OnAuthorizeEndpoint = SignInEpsilon; });

            OAuth2TestServer.Transaction transaction1 = await server.SendAsync("http://example.com/authorize?response_type=token&client_id=alpha&redirect_uri=" + Uri.EscapeDataString("http://gamma.com/return"));

            NameValueCollection fragment = transaction1.ParseRedirectFragment();
            string accessToken = fragment.Get("access_token");

            OAuth2TestServer.Transaction transaction2 = await server.SendAsync("http://example.com/me",
                authenticateHeader: new AuthenticationHeaderValue("Bearer", accessToken));

            transaction2.Response.StatusCode.ShouldBe(HttpStatusCode.OK);
            transaction2.ResponseText.ShouldBe("epsilon");
        }

        [Fact]
        public async Task UnrecognizedParametersAreIgnored()
        {
            var server = new OAuth2TestServer(s => { s.OnAuthorizeEndpoint = SignInEpsilon; });

            OAuth2TestServer.Transaction transaction1 = await server.SendAsync("http://example.com/authorize?alpha=beta&response_type=token&client_id=alpha&redirect_uri=" + Uri.EscapeDataString("http://gamma.com/return"));

            NameValueCollection fragment = transaction1.ParseRedirectFragment();

            string userName = await GetUserName(server, fragment.Get("access_token"));
            userName.ShouldBe("epsilon");
        }

        private Task SignInEpsilon(IOwinContext ctx)
        {
            ctx.Authentication.SignIn(new AuthenticationProperties(), CreateIdentity("epsilon"));
            return Task.FromResult(0);
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
            OAuth2TestServer.Transaction transaction = await server.SendAsync("http://example.com/me",
                authenticateHeader: new AuthenticationHeaderValue("Bearer", accessToken));

            transaction.Response.StatusCode.ShouldBe(HttpStatusCode.OK);
            return transaction.ResponseText;
        }
    }
}
