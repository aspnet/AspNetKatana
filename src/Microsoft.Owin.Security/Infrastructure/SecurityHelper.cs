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
        public AuthenticationResponseChallenge LookupChallenge(string authenticationType, AuthenticationMode authenticationMode)
        {
            if (authenticationType == null)
            {
                throw new ArgumentNullException("authenticationType");
            }

            AuthenticationResponseChallenge challenge = _response.AuthenticationResponseChallenge;
            bool challengeHasAuthenticationTypes = challenge != null && challenge.AuthenticationTypes != null && challenge.AuthenticationTypes.Length != 0;
            if (challengeHasAuthenticationTypes == false)
            {
                return authenticationMode == AuthenticationMode.Active ? challenge ?? new AuthenticationResponseChallenge(null, null) : null;
            }
            foreach (var challengeType in challenge.AuthenticationTypes)
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
        public AuthenticationResponseGrant LookupSignin(string authenticationType)
        {
            if (authenticationType == null)
            {
                throw new ArgumentNullException("authenticationType");
            }

            AuthenticationResponseGrant grant = _response.AuthenticationResponseGrant;
            if (grant == null)
            {
                return null;
            }

            foreach (var claimsIdentity in grant.Principal.Identities)
            {
                if (string.Equals(authenticationType, claimsIdentity.AuthenticationType, StringComparison.Ordinal))
                {
                    return new AuthenticationResponseGrant(claimsIdentity, grant.Extra ?? new AuthenticationExtra());
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
        public AuthenticationResponseRevoke LookupSignout(string authenticationType, AuthenticationMode authenticationMode)
        {
            if (authenticationType == null)
            {
                throw new ArgumentNullException("authenticationType");
            }

            AuthenticationResponseRevoke revoke = _response.AuthenticationResponseRevoke;
            if (revoke == null)
            {
                return null;
            }
            if (revoke.AuthenticationTypes == null || revoke.AuthenticationTypes.Length == 0)
            {
                return authenticationMode == AuthenticationMode.Active ? revoke : null;
            }
            for (int index = 0; index != revoke.AuthenticationTypes.Length; ++index)
            {
                if (String.Equals(authenticationType, revoke.AuthenticationTypes[index], StringComparison.Ordinal))
                {
                    return revoke;
                }
            }
            return null;
        }
    }
}
