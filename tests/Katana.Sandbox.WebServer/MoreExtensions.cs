// <copyright file="MoreExtensions.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Owin.Types;

namespace Katana.Sandbox.WebServer
{
    public static class MoreExtensions
    {
        public static async Task<ClaimsPrincipal> Authenticate2(this OwinRequest request, params string[] authenticationTypes)
        {
            var identities = new List<ClaimsIdentity>();
            await request.Authenticate(authenticationTypes, identity => identities.Add(new ClaimsIdentity(identity)));
            return identities.Count != 0 ? new ClaimsPrincipal(identities) : null;
        }
        public static async Task<ClaimsIdentity> AuthenticateSingle(this HttpContextBase request, string authenticationType)
        {
            var result = (await request.Authenticate(authenticationType)).ToArray().SingleOrDefault();
            return result == null ? null : new ClaimsIdentity(result.Identity);
        }

        public static void SignIn(this HttpContextBase context, string authenticationType, bool isPersistent, ClaimsIdentity identity)
        {
            var extra = new Dictionary<string, string>(StringComparer.Ordinal);
            if (isPersistent)
            {
                extra[".persistent"] = string.Empty;
            }
            context.SignIn(new ClaimsPrincipal(new ClaimsIdentity(identity.Claims, authenticationType, identity.NameClaimType, identity.RoleClaimType)), extra);
        }
        public static void SignIn(this HttpContextBase context, string authenticationType, bool isPersistent, params Claim[] claims)
        {
            SignIn(context, authenticationType, isPersistent, (IEnumerable<Claim>)claims);
        }
        public static void SignIn(this HttpContextBase context, string authenticationType, bool isPersistent, IEnumerable<Claim> claims)
        {
            var identity = new ClaimsIdentity(claims, authenticationType);
            var extra = new Dictionary<string, string>(StringComparer.Ordinal);
            if (isPersistent)
            {
                extra[".persistent"] = string.Empty;
            }
            context.SignIn(new ClaimsPrincipal(identity), extra);
        }
    }
}