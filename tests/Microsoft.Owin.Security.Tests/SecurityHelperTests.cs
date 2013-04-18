// <copyright file="SecurityHelperTests.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.Owin.Security.Infrastructure;
using Owin.Types;
using Owin.Types.Extensions;
using Shouldly;
using Xunit;

namespace Microsoft.Owin.Security.Tests
{
    public class SecurityHelperTests
    {
        [Fact]
        public void AddingToNullUserCreatesUserAsClaimsPrincipalWithSingleIdentity()
        {
            var request = OwinRequest.Create();
            request.User.ShouldBe(null);

            var helper = new SecurityHelper(request.Dictionary);
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
            var request = OwinRequest.Create();
            request.User = new GenericPrincipal(new GenericIdentity(string.Empty, string.Empty), null);
            request.User.Identity.IsAuthenticated.ShouldBe(false);

            var helper = new SecurityHelper(request.Dictionary);
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
            var request = OwinRequest.Create();
            request.User = new GenericPrincipal(new GenericIdentity("Test1", "Alpha"), null);
            var helper = new SecurityHelper(request.Dictionary);

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
            var request = OwinRequest.Create();
            var response = new OwinResponse(request);
            var helper = new SecurityHelper(request.Dictionary);

            var activeNoChallenge = helper.LookupChallenge("Alpha", AuthenticationMode.Active);
            var passiveNoChallenge = helper.LookupChallenge("Alpha", AuthenticationMode.Passive);

            response.Unauthorized();

            var activeEmptyChallenge = helper.LookupChallenge("Alpha", AuthenticationMode.Active);
            var passiveEmptyChallenge = helper.LookupChallenge("Alpha", AuthenticationMode.Passive);

            activeNoChallenge.ShouldNotBe(null);
            passiveNoChallenge.ShouldBe(null);
            activeEmptyChallenge.ShouldNotBe(null);
            passiveEmptyChallenge.ShouldBe(null);
        }

        [Fact]
        public void WithExtraDataMeansChallengesAreDeterminedOnlyByMatchingAuthenticationType()
        {
            var request = OwinRequest.Create();
            var response = new OwinResponse(request);
            var helper = new SecurityHelper(request.Dictionary);

            response.Unauthorized("Beta", "Gamma");

            var activeNoMatch = helper.LookupChallenge("Alpha", AuthenticationMode.Active);
            var passiveNoMatch = helper.LookupChallenge("Alpha", AuthenticationMode.Passive);

            response.Unauthorized("Beta", "Alpha");

            var activeWithMatch = helper.LookupChallenge("Alpha", AuthenticationMode.Active);
            var passiveWithMatch = helper.LookupChallenge("Alpha", AuthenticationMode.Passive);

            activeNoMatch.ShouldBe(null);
            passiveNoMatch.ShouldBe(null);
            activeWithMatch.ShouldNotBe(null);
            passiveWithMatch.ShouldNotBe(null);
        }
    }
}
