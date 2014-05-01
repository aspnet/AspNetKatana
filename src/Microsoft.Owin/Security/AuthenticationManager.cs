// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Microsoft.Owin.Security
{
    using AuthenticateDelegate = Func<string[], Action<IIdentity, IDictionary<string, string>, IDictionary<string, object>, object>, object, Task>;

    internal class AuthenticationManager : IAuthenticationManager
    {
        private readonly IOwinContext _context;
        private readonly IOwinRequest _request;

        public AuthenticationManager(IOwinContext context)
        {
            _context = context;
            _request = _context.Request;
        }

        public ClaimsPrincipal User
        {
            get
            {
                IPrincipal user = _request.User;
                if (user == null)
                {
                    return null;
                }
                return user as ClaimsPrincipal ?? new ClaimsPrincipal(user);
            }
            set { _request.User = value; }
        }

        internal AuthenticateDelegate AuthenticateDelegate
        {
            get { return _context.Get<AuthenticateDelegate>(OwinConstants.Security.Authenticate); }
        }

        /// <summary>
        /// Exposes the security.Challenge environment value as a strong type.
        /// </summary>
        public AuthenticationResponseChallenge AuthenticationResponseChallenge
        {
            get
            {
                Tuple<string[], IDictionary<string, string>> challenge = ChallengeEntry;
                if (challenge == null)
                {
                    return null;
                }
                return new AuthenticationResponseChallenge(challenge.Item1, new AuthenticationProperties(challenge.Item2));
            }
            set
            {
                if (value == null)
                {
                    ChallengeEntry = null;
                }
                else
                {
                    ChallengeEntry = Tuple.Create(value.AuthenticationTypes, value.Properties.Dictionary);
                }
            }
        }

        /// <summary>
        /// Exposes the security.SignIn environment value as a strong type.
        /// </summary>
        public AuthenticationResponseGrant AuthenticationResponseGrant
        {
            get
            {
                Tuple<IPrincipal, IDictionary<string, string>> grant = SignInEntry;
                if (grant == null)
                {
                    return null;
                }
                return new AuthenticationResponseGrant(grant.Item1 as ClaimsPrincipal ?? new ClaimsPrincipal(grant.Item1), new AuthenticationProperties(grant.Item2));
            }
            set
            {
                if (value == null)
                {
                    SignInEntry = null;
                }
                else
                {
                    SignInEntry = Tuple.Create((IPrincipal)value.Principal, value.Properties.Dictionary);
                }
            }
        }

        /// <summary>
        /// Exposes the security.SignOut environment value as a strong type.
        /// </summary>
        public AuthenticationResponseRevoke AuthenticationResponseRevoke
        {
            get
            {
                string[] revoke = SignOutEntry;
                if (revoke == null)
                {
                    return null;
                }
                return new AuthenticationResponseRevoke(revoke, new AuthenticationProperties(SignOutPropertiesEntry));
            }
            set
            {
                if (value == null)
                {
                    SignOutEntry = null;
                    SignOutPropertiesEntry = null;
                }
                else
                {
                    SignOutEntry = value.AuthenticationTypes;
                    SignOutPropertiesEntry = value.Properties.Dictionary;
                }
            }
        }

        public IEnumerable<AuthenticationDescription> GetAuthenticationTypes()
        {
            return GetAuthenticationTypes(_ => true);
        }

        public IEnumerable<AuthenticationDescription> GetAuthenticationTypes(Func<AuthenticationDescription, bool> predicate)
        {
            // TODO: refactor the signature to remove the .Wait() on this call path
            var descriptions = new List<AuthenticationDescription>();
            GetAuthenticationTypes(rawDescription =>
            {
                var description = new AuthenticationDescription(rawDescription);
                if (predicate(description))
                {
                    descriptions.Add(description);
                }
            }).Wait();
            return descriptions;
        }

        private Task GetAuthenticationTypes(Action<IDictionary<string, object>> callback)
        {
            return Authenticate(null, (_, __, description, ___) => callback(description), null);
        }

        public async Task<AuthenticateResult> AuthenticateAsync(string authenticationType)
        {
            return (await AuthenticateAsync(new[] { authenticationType })).SingleOrDefault();
        }

        public async Task<IEnumerable<AuthenticateResult>> AuthenticateAsync(string[] authenticationTypes)
        {
            var results = new List<AuthenticateResult>();
            await Authenticate(authenticationTypes, AuthenticateAsyncCallback, results);
            return results;
        }

        private static void AuthenticateAsyncCallback(IIdentity identity, IDictionary<string, string> properties, IDictionary<string, object> description, object state)
        {
            List<AuthenticateResult> list = (List<AuthenticateResult>)state;
            list.Add(new AuthenticateResult(identity, new AuthenticationProperties(properties), new AuthenticationDescription(description)));
        }

        public void Challenge(AuthenticationProperties properties, params string[] authenticationTypes)
        {
            _context.Response.StatusCode = 401;
            AuthenticationResponseChallenge priorChallenge = AuthenticationResponseChallenge;
            if (priorChallenge == null)
            {
                AuthenticationResponseChallenge = new AuthenticationResponseChallenge(authenticationTypes, properties);
            }
            else
            {
                // Cumulative auth types
                string[] mergedAuthTypes = priorChallenge.AuthenticationTypes.Concat(authenticationTypes).ToArray();

                if (properties != null && !object.ReferenceEquals(properties.Dictionary, priorChallenge.Properties.Dictionary))
                {
                    // Update prior properties
                    foreach (var propertiesPair in properties.Dictionary)
                    {
                        priorChallenge.Properties.Dictionary[propertiesPair.Key] = propertiesPair.Value;
                    }
                }

                AuthenticationResponseChallenge = new AuthenticationResponseChallenge(mergedAuthTypes, priorChallenge.Properties);
            }
        }

        public void Challenge(params string[] authenticationTypes)
        {
            Challenge(new AuthenticationProperties(), authenticationTypes);
        }

        public void SignIn(AuthenticationProperties properties, params ClaimsIdentity[] identities)
        {
            AuthenticationResponseRevoke priorRevoke = AuthenticationResponseRevoke;
            if (priorRevoke != null)
            {
                // Scan the sign-outs's and remove any with a matching auth type.
                string[] filteredSignOuts = priorRevoke.AuthenticationTypes
                    .Where(authType => !identities.Any(identity => identity.AuthenticationType.Equals(authType, StringComparison.Ordinal)))
                    .ToArray();
                if (filteredSignOuts.Length < priorRevoke.AuthenticationTypes.Length)
                {
                    if (filteredSignOuts.Length == 0)
                    {
                        AuthenticationResponseRevoke = null;
                    }
                    else
                    {
                        AuthenticationResponseRevoke = new AuthenticationResponseRevoke(filteredSignOuts);
                    }
                }
            }

            AuthenticationResponseGrant priorGrant = AuthenticationResponseGrant;
            if (priorGrant == null)
            {
                AuthenticationResponseGrant = new AuthenticationResponseGrant(new ClaimsPrincipal(identities), properties);
            }
            else
            {
                ClaimsIdentity[] mergedIdentities = priorGrant.Principal.Identities.Concat(identities).ToArray();

                if (properties != null && !object.ReferenceEquals(properties.Dictionary, priorGrant.Properties.Dictionary))
                {
                    // Update prior properties
                    foreach (var propertiesPair in properties.Dictionary)
                    {
                        priorGrant.Properties.Dictionary[propertiesPair.Key] = propertiesPair.Value;
                    }
                }

                AuthenticationResponseGrant = new AuthenticationResponseGrant(new ClaimsPrincipal(mergedIdentities), priorGrant.Properties);
            }
        }

        public void SignIn(params ClaimsIdentity[] identities)
        {
            SignIn(new AuthenticationProperties(), identities);
        }

        public void SignOut(AuthenticationProperties properties, string[] authenticationTypes)
        {
            AuthenticationResponseGrant priorGrant = AuthenticationResponseGrant;
            if (priorGrant != null)
            {
                // Scan the sign-in's and remove any with a matching auth type.
                ClaimsIdentity[] filteredIdentities = priorGrant.Principal.Identities
                    .Where(identity => !authenticationTypes.Contains(identity.AuthenticationType, StringComparer.Ordinal))
                    .ToArray();
                if (filteredIdentities.Length < priorGrant.Principal.Identities.Count())
                {
                    if (filteredIdentities.Length == 0)
                    {
                        AuthenticationResponseGrant = null;
                    }
                    else
                    {
                        AuthenticationResponseGrant = new AuthenticationResponseGrant(new ClaimsPrincipal(filteredIdentities), priorGrant.Properties);
                    }
                }
            }

            AuthenticationResponseRevoke priorRevoke = AuthenticationResponseRevoke;
            if (priorRevoke == null)
            {
                AuthenticationResponseRevoke = new AuthenticationResponseRevoke(authenticationTypes, properties);
            }
            else
            {
                if (properties != null && !object.ReferenceEquals(properties.Dictionary, priorRevoke.Properties.Dictionary))
                {
                    // Update prior properties
                    foreach (var propertiesPair in properties.Dictionary)
                    {
                        priorRevoke.Properties.Dictionary[propertiesPair.Key] = propertiesPair.Value;
                    }
                }

                // Cumulative auth types
                string[] mergedAuthTypes = priorRevoke.AuthenticationTypes.Concat(authenticationTypes).ToArray();
                AuthenticationResponseRevoke = new AuthenticationResponseRevoke(mergedAuthTypes, priorRevoke.Properties);
            }
        }

        public void SignOut(string[] authenticationTypes)
        {
            SignOut(new AuthenticationProperties(), authenticationTypes);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="authenticationTypes"></param>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public async Task Authenticate(string[] authenticationTypes, Action<IIdentity, IDictionary<string, string>, IDictionary<string, object>, object> callback, object state)
        {
            AuthenticateDelegate authenticateDelegate = AuthenticateDelegate;
            if (authenticateDelegate != null)
            {
                await authenticateDelegate.Invoke(authenticationTypes, callback, state);
            }
        }

        public Tuple<IPrincipal, IDictionary<string, string>> SignInEntry
        {
            get { return _context.Get<Tuple<IPrincipal, IDictionary<string, string>>>(OwinConstants.Security.SignIn); }
            set { _context.Set(OwinConstants.Security.SignIn, value); }
        }

        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays",
            Justification = "Using an array rather than a collection for this property for performance reasons.")]
        public string[] SignOutEntry
        {
            get { return _context.Get<string[]>(OwinConstants.Security.SignOut); }
            set { _context.Set(OwinConstants.Security.SignOut, value); }
        }

        public IDictionary<string, string> SignOutPropertiesEntry
        {
            get { return _context.Get<IDictionary<string, string>>(OwinConstants.Security.SignOutProperties); }
            set { _context.Set(OwinConstants.Security.SignOutProperties, value); }
        }

        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays",
            Justification = "Using an array rather than a collection for this property for performance reasons.")]
        public Tuple<string[], IDictionary<string, string>> ChallengeEntry
        {
            get { return _context.Get<Tuple<string[], IDictionary<string, string>>>(OwinConstants.Security.Challenge); }
            set { _context.Set(OwinConstants.Security.Challenge, value); }
        }
    }
}
