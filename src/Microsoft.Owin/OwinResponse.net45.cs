// <copyright file="OwinResponse.net45.cs" company="Microsoft Open Technologies, Inc.">
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

#if NET45

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.Owin.Security;

namespace Microsoft.Owin
{
    public partial struct OwinResponse
    {
        /// <summary>
        /// 
        /// </summary>
        public AuthenticationResponseChallenge AuthenticationResponseChallenge
        {
            get
            {
                Tuple<string[], IDictionary<string, string>> challenge = _response.Challenge;
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
                    _response.Challenge = null;
                }
                else
                {
                    _response.Challenge = Tuple.Create(value.AuthenticationTypes, value.Extra.Properties);
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
                Tuple<IPrincipal, IDictionary<string, string>> grant = _response.SignIn;
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
                    _response.SignIn = null;
                }
                else
                {
                    _response.SignIn = Tuple.Create((IPrincipal)value.Principal, value.Extra.Properties);
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
                string[] revoke = _response.SignOut;
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
                    _response.SignOut = null;
                }
                else
                {
                    _response.SignOut = value.AuthenticationTypes;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="identity"></param>
        public void Grant(ClaimsIdentity identity)
        {
            AuthenticationResponseGrant = new AuthenticationResponseGrant(identity, new AuthenticationExtra());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="extra"></param>
        public void Grant(ClaimsIdentity identity, AuthenticationExtra extra)
        {
            AuthenticationResponseGrant = new AuthenticationResponseGrant(identity, extra);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="principal"></param>
        public void Grant(ClaimsPrincipal principal)
        {
            AuthenticationResponseGrant = new AuthenticationResponseGrant(principal, new AuthenticationExtra());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="principal"></param>
        /// <param name="extra"></param>
        public void Grant(ClaimsPrincipal principal, AuthenticationExtra extra)
        {
            AuthenticationResponseGrant = new AuthenticationResponseGrant(principal, extra);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="authenticationTypes"></param>
        public void Challenge(string[] authenticationTypes)
        {
            StatusCode = 401;
            AuthenticationResponseChallenge = new AuthenticationResponseChallenge(authenticationTypes, new AuthenticationExtra());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="authenticationTypes"></param>
        /// <param name="extra"></param>
        public void Challenge(string[] authenticationTypes, AuthenticationExtra extra)
        {
            StatusCode = 401;
            AuthenticationResponseChallenge = new AuthenticationResponseChallenge(authenticationTypes, extra);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="authenticationTypes"></param>
        public void Revoke(string[] authenticationTypes)
        {
            AuthenticationResponseRevoke = new AuthenticationResponseRevoke(authenticationTypes);
        }
    }
}

#endif
