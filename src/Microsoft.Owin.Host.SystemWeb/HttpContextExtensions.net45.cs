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
using System.Threading.Tasks;
using Microsoft.Owin.Host.SystemWeb;

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
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Following Owin conventions.")]
        public static async Task<IEnumerable<AuthenticationTypeMetadata>> GetAuthenticationTypes(
            this HttpContext context)
        {
            List<AuthenticationTypeMetadata> authenticationTypes = new List<AuthenticationTypeMetadata>();
            await GetAuthenticationTypes(context, (properties, ignore) =>
            {
                authenticationTypes.Add(new AuthenticationTypeMetadata(properties));
            }, null);
            return authenticationTypes;
        }
    }
}

#else

using ResharperCodeFormattingWorkaround = System.Object;

#endif
