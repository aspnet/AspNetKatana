using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using Owin.Types;

namespace Microsoft.Owin.Security.Infrastructure
{
    public struct SecurityHelper
    {
        private OwinRequest _request;
        private OwinResponse _response;

        public SecurityHelper(IDictionary<string, object> environment)
        {
            _request = new OwinRequest(environment);
            _response = new OwinResponse(environment);
        }

        public void AddUserIdentity(IIdentity identity)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }
            var newClaimsPrincipal = new ClaimsPrincipal(identity);

            var existingPrincipal = _request.User;
            if (existingPrincipal != null)
            {
                var existingClaimsPrincipal = existingPrincipal as ClaimsPrincipal;
                if (existingClaimsPrincipal == null)
                {
                    var existingIdentity = existingPrincipal.Identity;
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

        public SecurityHelperLookupResult LookupChallenge(string authenticationType, AuthenticationMode authenticationMode)
        {
            if (authenticationType == null)
            {
                throw new ArgumentNullException("authenticationType");
            }

            var challenge = DoLookup(_response.Challenge, authenticationType, authenticationMode, false);
            return challenge;
        }

        public SecurityHelperLookupResult LookupSignin(string authenticationType)
        {
            if (authenticationType == null)
            {
                throw new ArgumentNullException("authenticationType");
            }

            var signin = DoLookup(_response.Grant, authenticationType, AuthenticationMode.Passive, true);
            return signin;
        }

        public SecurityHelperLookupResult LookupSignout(string authenticationType, AuthenticationMode authenticationMode)
        {
            if (authenticationType == null)
            {
                throw new ArgumentNullException("authenticationType");
            }

            var signout = DoLookup(_response.Get<IPrincipal>("security.Signout"), authenticationType, authenticationMode, true);
            return signout;
        }

        private static SecurityHelperLookupResult DoLookup(
            IPrincipal principal, 
            string authenticationType, 
            AuthenticationMode authenticationMode, 
            bool principalRequired)
        {
            if (principal == null)
            {
                return new SecurityHelperLookupResult(!principalRequired && authenticationMode == AuthenticationMode.Active);
            }

            bool performedComparison = false;

            var claimsPrincipal = principal as ClaimsPrincipal;
            if (claimsPrincipal == null)
            {
                performedComparison = true;
                if (string.Equals(principal.Identity.AuthenticationType, authenticationType, StringComparison.Ordinal))
                {
                    var identity = principal.Identity as ClaimsIdentity ?? new ClaimsIdentity(principal.Identity);
                    return new SecurityHelperLookupResult(true, identity);
                }
            }
            else
            {
                foreach (var identity in claimsPrincipal.Identities)
                {
                    performedComparison = true;
                    if (string.Equals(identity.AuthenticationType, authenticationType, StringComparison.Ordinal))
                    {
                        return new SecurityHelperLookupResult(true, identity);
                    }
                }
            }

            return performedComparison
                ? new SecurityHelperLookupResult(false)
                : new SecurityHelperLookupResult(authenticationMode == AuthenticationMode.Active);
        }
    }
}
