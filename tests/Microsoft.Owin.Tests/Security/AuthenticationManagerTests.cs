// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Security.Claims;
using Microsoft.Owin.Security;
using Xunit;

namespace Microsoft.Owin.Tests.Security
{
    public class AuthenticationManagerTests
    {
        [Fact]
        public void NullUserReturnsNull()
        {
            IOwinContext context = new OwinContext();
            Assert.Null(context.Request.User);
            Assert.Null(context.Authentication.User);
        }

        [Fact]
        public void Challenge_SetsKey()
        {
            IOwinContext context = new OwinContext();
            context.Authentication.Challenge("foo", "bar");
            Assert.Equal(401, context.Response.StatusCode);
            AuthenticationResponseChallenge challange = context.Authentication.AuthenticationResponseChallenge;
            Assert.Equal(new[] { "foo", "bar" }, challange.AuthenticationTypes);
            Assert.NotNull(challange.Properties);
        }

        [Fact]
        public void ChallengeTwice_Cumulative()
        {
            IOwinContext context = new OwinContext();
            context.Authentication.Challenge("foo");
            context.Authentication.Challenge("bar");
            Assert.Equal(401, context.Response.StatusCode);
            AuthenticationResponseChallenge challange = context.Authentication.AuthenticationResponseChallenge;
            Assert.Equal(new[] { "foo", "bar" }, challange.AuthenticationTypes);
            Assert.NotNull(challange.Properties);
        }

        [Fact]
        public void SignIn_SetsKey()
        {
            IOwinContext context = new OwinContext();
            context.Authentication.SignIn(new ClaimsIdentity("foo"), new ClaimsIdentity("bar"));
            AuthenticationResponseGrant grant = context.Authentication.AuthenticationResponseGrant;
            Assert.Equal("foo", grant.Principal.Identities.First().AuthenticationType);
            Assert.Equal("bar", grant.Principal.Identities.Skip(1).First().AuthenticationType);
            Assert.NotNull(grant.Properties);
        }

        [Fact]
        public void SignInTwice_Cumulative()
        {
            IOwinContext context = new OwinContext();
            context.Authentication.SignIn(new ClaimsIdentity("foo"));
            context.Authentication.SignIn(new ClaimsIdentity("bar"));
            AuthenticationResponseGrant grant = context.Authentication.AuthenticationResponseGrant;
            Assert.Equal("foo", grant.Principal.Identities.First().AuthenticationType);
            Assert.Equal("bar", grant.Principal.Identities.Skip(1).First().AuthenticationType);
            Assert.NotNull(grant.Properties);
        }

        [Fact]
        public void SignOut_SetsKey()
        {
            IOwinContext context = new OwinContext();
            context.Authentication.SignOut("foo", "bar");
            Assert.Equal(new[] { "foo", "bar" }, context.Get<string[]>("security.SignOut"));
        }

        [Fact]
        public void SignOutTwice_Cumulative()
        {
            IOwinContext context = new OwinContext();
            context.Authentication.SignOut("foo");
            context.Authentication.SignOut("bar");
            Assert.Equal(new[] { "foo", "bar" }, context.Get<string[]>("security.SignOut"));
        }

        [Fact]
        public void SignInAndSignOut_Deduplicates()
        {
            IOwinContext context = new OwinContext();
            context.Authentication.SignIn(new ClaimsIdentity("foo"), new ClaimsIdentity("bar"));
            context.Authentication.SignOut("foo");

            AuthenticationResponseGrant grant = context.Authentication.AuthenticationResponseGrant;
            Assert.Equal("bar", grant.Principal.Identities.First().AuthenticationType);

            Assert.Equal(new[] { "foo" }, context.Get<string[]>("security.SignOut"));
            Assert.NotNull(grant.Properties);
        }

        [Fact]
        public void SignOutAndSignIn_Deduplicates()
        {
            IOwinContext context = new OwinContext();
            context.Authentication.SignOut("foo", "bar");
            context.Authentication.SignIn(new ClaimsIdentity("foo"));

            AuthenticationResponseGrant grant = context.Authentication.AuthenticationResponseGrant;
            Assert.Equal("foo", grant.Principal.Identities.First().AuthenticationType);

            Assert.Equal(new[] { "bar" }, context.Get<string[]>("security.SignOut"));
            Assert.NotNull(grant.Properties);
        }
    }
}
