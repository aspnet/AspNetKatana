// <copyright file="HttpContextExtensions.net45.cs" company="Microsoft Open Technologies, Inc.">
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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Owin.Host.SystemWeb;
using Microsoft.Owin.Security;

namespace System.Web
{
    public static partial class HttpContextExtensions
    {
        /// <summary></summary>
        /// <param name="context"></param>
        /// <param name="authenticationTypes"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Following Owin conventions.")]
        public static async Task<IEnumerable<AuthenticationResult>> Authenticate(this HttpContext context,
            params string[] authenticationTypes)
        {
            List<AuthenticationResult> results = new List<AuthenticationResult>();
            await Authenticate(context, authenticationTypes, (identity, extra, properties, ignore) =>
                {
                    results.Add(new AuthenticationResult(identity, extra, properties));
                }, null);
            return results;
        }

        /// <summary></summary>
        /// <param name="context"></param>
        /// <param name="authenticationType"></param>
        /// <returns></returns>
        public static async Task<ClaimsIdentity> Authenticate(this HttpContext context, string authenticationType)
        {
            ClaimsIdentity claimsIdentity = null;
            await Authenticate(context, new[] { authenticationType }, (identity, extra, properties, ignore) =>
            {
                if (claimsIdentity == null)
                {
                    claimsIdentity = identity as ClaimsIdentity ?? new ClaimsIdentity(identity);
                }
            }, null);
            return claimsIdentity;
        }

        /// <summary></summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Following Owin conventions.")]
        public static IEnumerable<AuthenticationDescription> GetAuthenticationTypes(
            this HttpContext context)
        {
            List<AuthenticationDescription> authenticationTypes = new List<AuthenticationDescription>();
            GetAuthenticationTypes(context, (properties, ignore) =>
            {
                authenticationTypes.Add(new AuthenticationDescription(properties));
            }, null).Wait();
            return authenticationTypes;
        }

        /// <summary></summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Following Owin conventions.")]
        public static IEnumerable<AuthenticationDescription> GetExternalAuthenticationTypes(
            this HttpContext context)
        {
            List<AuthenticationDescription> authenticationTypes = new List<AuthenticationDescription>();
            GetAuthenticationTypes(context, (properties, ignore) =>
            {
                if (properties != null && properties.ContainsKey(Constants.CaptionKey))
                {
                    authenticationTypes.Add(new AuthenticationDescription(properties));
                }
            }, null).Wait();
            return authenticationTypes;
        }

        /// <summary></summary>
        /// <param name="context"></param>
        /// <param name="claims"></param>
        /// <param name="nameClaimType"></param>
        /// <param name="roleClaimType"></param>
        /// <param name="isPersistent"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void SignIn(this HttpContext context, IEnumerable<Claim> claims, string nameClaimType, string roleClaimType, bool isPersistent)
        {
            SignIn(context, Constants.ApplicationAuthenticationType, claims, nameClaimType, roleClaimType, isPersistent);
        }

        /// <summary></summary>
        /// <param name="context"></param>
        /// <param name="authenticationType"> </param>
        /// <param name="claims"></param>
        /// <param name="nameClaimType"></param>
        /// <param name="roleClaimType"></param>
        /// <param name="isPersistent"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void SignIn(this HttpContext context, string authenticationType, IEnumerable<Claim> claims, string nameClaimType, string roleClaimType, bool isPersistent)
        {
            if (authenticationType == null)
            {
                throw new ArgumentNullException("authenticationType");
            }
            if (claims == null)
            {
                throw new ArgumentNullException("claims");
            }
            if (nameClaimType == null)
            {
                throw new ArgumentNullException("nameClaimType");
            }
            if (roleClaimType == null)
            {
                throw new ArgumentNullException("roleClaimType");
            }
            var extra = new AuthenticationExtra { IsPersistent = isPersistent };
            context.SignIn(new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType, nameClaimType, roleClaimType)), extra);
        }
    }
}

#else

using ResharperCodeFormattingWorkaround = System.Object;

#endif
