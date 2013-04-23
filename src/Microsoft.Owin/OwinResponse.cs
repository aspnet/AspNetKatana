// <copyright file="OwinResponse.cs" company="Microsoft Open Technologies, Inc.">
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
using System.IO;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.Owin.Security;
using Owin.Types.Extensions;

namespace Microsoft.Owin
{
    public struct OwinResponse
    {
        private global::Owin.Types.OwinResponse _response;

        public OwinResponse(IDictionary<string, object> environment)
        {
            _response = new global::Owin.Types.OwinResponse(environment);
        }

        public OwinResponse(OwinRequest request) : this(request.Environment)
        {
        }

        public IDictionary<string, object> Environment
        {
            get { return _response.Dictionary; }
        }

        public int StatusCode
        {
            get { return _response.StatusCode; }
            set { _response.StatusCode = value; }
        }

        public AuthenticationResponseChallenge AuthenticationResponseChallenge
        {
            get
            {
                Tuple<string[], IDictionary<string, string>> challenge = _response.Challenge;
                if (challenge == null)
                {
                    return null;
                }
                return new AuthenticationResponseChallenge(challenge.Item1, challenge.Item2);
            }
            set
            {
                if (value == null)
                {
                    _response.Challenge = null;
                }
                else
                {
                    _response.Challenge = Tuple.Create(value.AuthenticationTypes, value.Extra);
                }
            }
        }

        public AuthenticationResponseGrant AuthenticationResponseGrant
        {
            get
            {
                Tuple<IPrincipal, IDictionary<string, string>> grant = _response.SignIn;
                if (grant == null)
                {
                    return null;
                }
                return new AuthenticationResponseGrant(grant.Item1 as ClaimsPrincipal ?? new ClaimsPrincipal(grant.Item1), grant.Item2);
            }
            set
            {
                if (value == null)
                {
                    _response.SignIn = null;
                }
                else
                {
                    _response.SignIn = Tuple.Create((IPrincipal)value.Principal, value.Extra);
                }
            }
        }

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

        public string ContentType
        {
            get { return _response.ContentType; }
            set { _response.ContentType = value; }
        }

        public Stream Body
        {
            get { return _response.Body; }
            set { _response.Body = value; }
        }

        public void AddCookie(string cookieName, string cookieValue)
        {
            _response.AddCookie(cookieName, cookieValue);
        }

        public void AddCookie(string cookieName, string cookieValue, CookieOptions cookieOptions)
        {
            if (cookieOptions == null)
            {
                throw new ArgumentNullException("cookieOptions");
            }
            _response.AddCookie(cookieName, cookieValue, new global::Owin.Types.Helpers.CookieOptions
            {
                Domain = cookieOptions.Domain,
                Path = cookieOptions.Path,
                Expires = cookieOptions.Expires,
                Secure = cookieOptions.Secure,
                HttpOnly = cookieOptions.HttpOnly,
            });
        }

        public void DeleteCookie(string cookieName)
        {
            _response.DeleteCookie(cookieName);
        }

        public void DeleteCookie(string cookieName, CookieOptions cookieOptions)
        {
            if (cookieOptions == null)
            {
                throw new ArgumentNullException("cookieOptions");
            }
            _response.DeleteCookie(cookieName, new global::Owin.Types.Helpers.CookieOptions
            {
                Domain = cookieOptions.Domain,
                Path = cookieOptions.Path,
                Expires = cookieOptions.Expires,
                Secure = cookieOptions.Secure,
                HttpOnly = cookieOptions.HttpOnly,
            });
        }

        public void AddHeader(string name, string value)
        {
            _response.AddHeader(name, value);
        }

        public void SetHeader(string name, string value)
        {
            _response.SetHeader(name, value);
        }

        public void Redirect(string location)
        {
            _response.Redirect(location);
        }

        public void Grant(ClaimsIdentity identity, IDictionary<string, string> extra)
        {
            AuthenticationResponseGrant = new AuthenticationResponseGrant(identity, extra);
        }

        public void Grant(ClaimsPrincipal principal, IDictionary<string, string> extra)
        {
            AuthenticationResponseGrant = new AuthenticationResponseGrant(principal, extra);
        }

        public void Challenge(params string[] authenticationTypes)
        {
            StatusCode = 401;
            AuthenticationResponseChallenge = new AuthenticationResponseChallenge(
                authenticationTypes,
                new Dictionary<string, string>(StringComparer.Ordinal));
        }
    }
}
