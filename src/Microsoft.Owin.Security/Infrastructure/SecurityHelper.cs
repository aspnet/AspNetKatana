// <copyright file="SecurityHelper.cs" company="Microsoft Open Technologies, Inc.">
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

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using Owin.Types;
using IdentityModelClaim = System.IdentityModel.Claims.Claim;

namespace Microsoft.Owin.Security.Infrastructure
{
    /// <summary>
    /// Helper code used when implementing authentication middleware
    /// </summary>
    public struct SecurityHelper
    {
        private OwinRequest _request;
        private OwinResponse _response;

        /// <summary>
        /// Helper code used when implementing authentication middleware
        /// </summary>
        /// <param name="environment"></param>
        public SecurityHelper(IDictionary<string, object> environment)
        {
            _request = new OwinRequest(environment);
            _response = new OwinResponse(environment);
        }

        /// <summary>
        /// Add an additional ClaimsIdentity to the ClaimsPrincipal in the "server.User" environment key
        /// </summary>
        /// <param name="identity"></param>
        public void AddUserIdentity(IIdentity identity)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }
            var newClaimsPrincipal = new ClaimsPrincipal(identity);

            IPrincipal existingPrincipal = _request.User;
            if (existingPrincipal != null)
            {
                var existingClaimsPrincipal = existingPrincipal as ClaimsPrincipal;
                if (existingClaimsPrincipal == null)
                {
                    IIdentity existingIdentity = existingPrincipal.Identity;
                    if (existingIdentity.IsAuthenticated)
                    {
                        newClaimsPrincipal.AddIdentity(existingIdentity as ClaimsIdentity ?? new ClaimsIdentity(existingIdentity));
                    }
                }
                else
                {
                    foreach (var existingClaimsIdentity in existingClaimsPrincipal.Identities)
                    {
                        if (existingClaimsIdentity.IsAuthenticated)
                        {
                            newClaimsPrincipal.AddIdentity(existingClaimsIdentity);
                        }
                    }
                }
            }
            _request.User = newClaimsPrincipal;
        }

        /// <summary>
        /// Find response challenge details for a specific authentication middleware
        /// </summary>
        /// <param name="authenticationType">The authentication type to look for</param>
        /// <param name="authenticationMode">The authentication mode the middleware is running under</param>
        /// <returns>The information instructing the middleware how it should behave</returns>
        public Tuple<string[], IDictionary<string, string>> LookupChallenge(string authenticationType, AuthenticationMode authenticationMode)
        {
            if (authenticationType == null)
            {
                throw new ArgumentNullException("authenticationType");
            }

            Tuple<string[], IDictionary<string, string>> challenge = _response.Challenge;
            bool challengeHasAuthenticationTypes = challenge != null && challenge.Item1 != null && challenge.Item1.Length != 0;
            if (challengeHasAuthenticationTypes == false)
            {
                return authenticationMode == AuthenticationMode.Active ? challenge ?? new Tuple<string[], IDictionary<string, string>>(null, null) : null;
            }
            foreach (var challengeType in challenge.Item1)
            {
                if (string.Equals(challengeType, authenticationType, StringComparison.Ordinal))
                {
                    return challenge;
                }
            }
            return null;
        }

        /// <summary>
        /// Find response signin details for a specific authentication middleware
        /// </summary>
        /// <param name="authenticationType">The authentication type to look for</param>
        /// <returns>The information instructing the middleware how it should behave</returns>
        public Tuple<IIdentity, IDictionary<string, string>> LookupSignin(string authenticationType)
        {
            if (authenticationType == null)
            {
                throw new ArgumentNullException("authenticationType");
            }

            Tuple<IPrincipal, IDictionary<string, string>> signIn = _response.SignIn;
            if (signIn == null)
            {
                return null;
            }

            IPrincipal principal = signIn.Item1;
            var claimsPrincipal = principal as ClaimsPrincipal;
            if (claimsPrincipal == null)
            {
                if (string.Equals(authenticationType, principal.Identity.AuthenticationType, StringComparison.Ordinal))
                {
                    return Tuple.Create(principal.Identity, signIn.Item2);
                }
                return null;
            }

            foreach (var claimsIdentity in claimsPrincipal.Identities)
            {
                if (string.Equals(authenticationType, claimsIdentity.AuthenticationType, StringComparison.Ordinal))
                {
                    return Tuple.Create((IIdentity)claimsIdentity, signIn.Item2);
                }
            }

            return null;
        }

        /// <summary>
        /// Find response signout details for a specific authentication middleware
        /// </summary>
        /// <param name="authenticationType">The authentication type to look for</param>
        /// <param name="authenticationMode">The authentication mode the middleware is running under</param>
        /// <returns>The information instructing the middleware how it should behave</returns>
        public bool LookupSignout(string authenticationType, AuthenticationMode authenticationMode)
        {
            if (authenticationType == null)
            {
                throw new ArgumentNullException("authenticationType");
            }

            string[] signOut = _response.SignOut;
            if (signOut == null)
            {
                return false;
            }
            if (signOut.Length == 0)
            {
                return authenticationMode == AuthenticationMode.Active;
            }
            for (int index = 0; index != signOut.Length; ++index)
            {
                if (String.Equals(authenticationType, signOut[index], StringComparison.Ordinal))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
