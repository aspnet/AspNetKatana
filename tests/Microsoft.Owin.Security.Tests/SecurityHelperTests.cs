// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.Owin.Security.Infrastructure;
using Shouldly;
using Xunit;

namespace Microsoft.Owin.Security.Tests
{
    public class SecurityHelperTests
    {
        [Fact]
        public void AddingToNullUserCreatesUserAsClaimsPrincipalWithSingleIdentity()
        {
            IOwinContext context = new OwinContext();
            IOwinRequest request = context.Request;
            request.User.ShouldBe(null);

            var helper = new SecurityHelper(context);
            helper.AddUserIdentity(new GenericIdentity("Test1", "Alpha"));

            request.User.ShouldNotBe(null);
            request.User.Identity.AuthenticationType.ShouldBe("Alpha");
            request.User.Identity.Name.ShouldBe("Test1");

            request.User.ShouldBeTypeOf<ClaimsPrincipal>();
            request.User.Identity.ShouldBeTypeOf<ClaimsIdentity>();

            ((ClaimsPrincipal)request.User).Identities.Count().ShouldBe(1);
        }

        [Fact]
        public void AddingToAnonymousIdentityDoesNotKeepAnonymousIdentity()
        {
            IOwinContext context = new OwinContext();
            IOwinRequest request = context.Request;
            request.User = new GenericPrincipal(new GenericIdentity(string.Empty, string.Empty), null);
            request.User.Identity.IsAuthenticated.ShouldBe(false);

            var helper = new SecurityHelper(context);
            helper.AddUserIdentity(new GenericIdentity("Test1", "Alpha"));

            request.User.ShouldNotBe(null);
            request.User.Identity.AuthenticationType.ShouldBe("Alpha");
            request.User.Identity.Name.ShouldBe("Test1");

            request.User.ShouldBeTypeOf<ClaimsPrincipal>();
            request.User.Identity.ShouldBeTypeOf<ClaimsIdentity>();

            ((ClaimsPrincipal)request.User).Identities.Count().ShouldBe(1);
        }

        [Fact]
        public void AddingExistingIdentityChangesDefaultButPreservesPrior()
        {
            IOwinContext context = new OwinContext();
            IOwinRequest request = context.Request;
            request.User = new GenericPrincipal(new GenericIdentity("Test1", "Alpha"), null);
            var helper = new SecurityHelper(context);

            request.User.Identity.AuthenticationType.ShouldBe("Alpha");
            request.User.Identity.Name.ShouldBe("Test1");

            helper.AddUserIdentity(new GenericIdentity("Test2", "Beta"));

            request.User.Identity.AuthenticationType.ShouldBe("Beta");
            request.User.Identity.Name.ShouldBe("Test2");

            helper.AddUserIdentity(new GenericIdentity("Test3", "Gamma"));

            request.User.Identity.AuthenticationType.ShouldBe("Gamma");
            request.User.Identity.Name.ShouldBe("Test3");

            var principal = (ClaimsPrincipal)request.User;
            principal.Identities.Count().ShouldBe(3);
            principal.Identities.Skip(0).First().Name.ShouldBe("Test3");
            principal.Identities.Skip(1).First().Name.ShouldBe("Test2");
            principal.Identities.Skip(2).First().Name.ShouldBe("Test1");
        }

        [Fact]
        public void NoExtraDataMeansChallengesAreDeterminedOnlyByActiveOrPassiveMode()
        {
            IOwinContext context = new OwinContext();
            IOwinRequest request = context.Request;
            IOwinResponse response = context.Response;
            var helper = new SecurityHelper(context);

            AuthenticationResponseChallenge activeNoChallenge = helper.LookupChallenge("Alpha", AuthenticationMode.Active);
            AuthenticationResponseChallenge passiveNoChallenge = helper.LookupChallenge("Alpha", AuthenticationMode.Passive);

            response.StatusCode = 401;

            AuthenticationResponseChallenge activeEmptyChallenge = helper.LookupChallenge("Alpha", AuthenticationMode.Active);
            AuthenticationResponseChallenge passiveEmptyChallenge = helper.LookupChallenge("Alpha", AuthenticationMode.Passive);

            activeNoChallenge.ShouldNotBe(null);
            passiveNoChallenge.ShouldBe(null);
            activeEmptyChallenge.ShouldNotBe(null);
            passiveEmptyChallenge.ShouldBe(null);
        }

        [Fact]
        public void WithExtraDataMeansChallengesAreDeterminedOnlyByMatchingAuthenticationType()
        {
            IOwinContext context = new OwinContext();
            IOwinRequest request = context.Request;
            IOwinResponse response = context.Response;
            var helper = new SecurityHelper(context);

            response.Authentication.Challenge(new AuthenticationProperties(), "Beta", "Gamma");

            AuthenticationResponseChallenge activeNoMatch = helper.LookupChallenge("Alpha", AuthenticationMode.Active);
            AuthenticationResponseChallenge passiveNoMatch = helper.LookupChallenge("Alpha", AuthenticationMode.Passive);

            context.Authentication.Challenge(new AuthenticationProperties(), "Beta", "Alpha");

            AuthenticationResponseChallenge activeWithMatch = helper.LookupChallenge("Alpha", AuthenticationMode.Active);
            AuthenticationResponseChallenge passiveWithMatch = helper.LookupChallenge("Alpha", AuthenticationMode.Passive);

            activeNoMatch.ShouldBe(null);
            passiveNoMatch.ShouldBe(null);
            activeWithMatch.ShouldNotBe(null);
            passiveWithMatch.ShouldNotBe(null);
        }
    }
}
