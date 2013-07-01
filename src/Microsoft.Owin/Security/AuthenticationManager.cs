// <copyright file="AuthenticationManager.cs" company="Microsoft Open Technologies, Inc.">
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

#if !NET40

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using AuthenticateDelegate = System.Func<string[], System.Action<System.Security.Principal.IIdentity, System.Collections.Generic.IDictionary<string, string>, System.Collections.Generic.IDictionary<string, object>, object>, object, System.Threading.Tasks.Task>;

namespace Microsoft.Owin.Security
{
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
            get { return _request.User as ClaimsPrincipal ?? new ClaimsPrincipal(_request.User); }
            set { _request.User = value; }
        }

        internal AuthenticateDelegate AuthenticateDelegate
        {
            get { return _context.Get<AuthenticateDelegate>(OwinConstants.Security.Authenticate); }
        }

        /// <summary>
        /// 
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
                return new AuthenticationResponseChallenge(challenge.Item1, new AuthenticationExtra(challenge.Item2));
            }
            set
            {
                if (value == null)
                {
                    ChallengeEntry = null;
                }
                else
                {
                    ChallengeEntry = Tuple.Create(value.AuthenticationTypes, value.Extra.Properties);
                }
            }
        }

        /// <summary>
        /// 
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
                return new AuthenticationResponseGrant(grant.Item1 as ClaimsPrincipal ?? new ClaimsPrincipal(grant.Item1), new AuthenticationExtra(grant.Item2));
            }
            set
            {
                if (value == null)
                {
                    SignInEntry = null;
                }
                else
                {
                    SignInEntry = Tuple.Create((IPrincipal)value.Principal, value.Extra.Properties);
                }
            }
        }

        /// <summary>
        /// 
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
                return new AuthenticationResponseRevoke(revoke);
            }
            set
            {
                if (value == null)
                {
                    SignOutEntry = null;
                }
                else
                {
                    SignOutEntry = value.AuthenticationTypes;
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
            GetAuthenticationTypes((properties, _) =>
            {
                var description = new AuthenticationDescription(properties);
                if (predicate(description))
                {
                    descriptions.Add(description);
                }
            }, null).Wait();
            return descriptions;
        }

        private Task GetAuthenticationTypes(Action<IDictionary<string, object>, object> callback, object state)
        {
            return Authenticate(null, (_, __, properties, ___) => callback(properties, state), null);
        }

        public async Task<AuthenticateResult> AuthenticateAsync(string authenticationType)
        {
            return (await AuthenticateAsync(new[] { authenticationType })).SingleOrDefault();
        }

        public async Task<IEnumerable<AuthenticateResult>> AuthenticateAsync(string[] authenticationTypes)
        {
            var descriptions = new List<AuthenticateResult>();
            await Authenticate(authenticationTypes,
                (identity, extra, description, state) => ((List<AuthenticateResult>)state).Add(new AuthenticateResult(identity, extra, description)), descriptions);
            return descriptions;
        }

        public void Challenge(AuthenticationExtra extra, params string[] authenticationTypes)
        {
            _context.Response.StatusCode = 401;
            AuthenticationResponseChallenge = new AuthenticationResponseChallenge(authenticationTypes, extra);
        }

        public void SignIn(AuthenticationExtra extra, params ClaimsIdentity[] identities)
        {
            AuthenticationResponseGrant = new AuthenticationResponseGrant(new ClaimsPrincipal(identities), extra);
        }

        public void SignOut(string[] authenticationTypes)
        {
            AuthenticationResponseRevoke = new AuthenticationResponseRevoke(authenticationTypes);
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
            var authenticateDelegate = AuthenticateDelegate;
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

        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays",
            Justification = "Using an array rather than a collection for this property for performance reasons.")]
        public Tuple<string[], IDictionary<string, string>> ChallengeEntry
        {
            get { return _context.Get<Tuple<string[], IDictionary<string, string>>>(OwinConstants.Security.Challenge); }
            set { _context.Set(OwinConstants.Security.Challenge, value); }
        }
    }
}
#endif
