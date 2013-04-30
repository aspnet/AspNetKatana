// <copyright file="OwinRequestExtensions.cs" company="Microsoft Open Technologies, Inc.">
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

using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;

namespace Microsoft.Owin.Security
{
    public static class OwinResponseExtensions
    {
        public static void SignIn(this OwinResponse response, IPrincipal user)
        {
            SignIn(response, user, null);
        }

        public static void SignIn(this OwinResponse response, IPrincipal user, IDictionary<string, string> extra)
        {
            response.AuthenticationResponseGrant = new AuthenticationResponseGrant(user as ClaimsPrincipal ?? new ClaimsPrincipal(user), extra);
        }

        public static void SignOut(this OwinResponse response, params string[] authenticationTypes)
        {
            response.AuthenticationResponseRevoke = new AuthenticationResponseRevoke(authenticationTypes);
        }

        public static void Unauthorized(this OwinResponse response, params string[] authenticationTypes)
        {
            Unauthorized(response, authenticationTypes, null);
        }

        public static void Unauthorized(this OwinResponse response, string[] authenticationTypes, IDictionary<string, string> extra)
        {
            response.StatusCode = 401;
            response.AuthenticationResponseChallenge = new AuthenticationResponseChallenge(authenticationTypes, extra);
        }
    }
}

