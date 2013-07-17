// <copyright file="AuthenticateResult.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;

namespace Microsoft.Owin.Security
{
    /// <summary>
    /// Acts as the return value from calls to the IAuthenticationManager's AuthenticeAsync methods.
    /// </summary>
    public class AuthenticateResult
    {
        /// <summary>
        /// Create an instance of the result object
        /// </summary>
        /// <param name="identity">Assigned to the Identity property. May be null.</param>
        /// <param name="extra">Assigned to the Extra property. An empty Extra instance is created if needed.</param>
        /// <param name="description">Assigned to the Description property. An empty AuthenticationDescription instance is created if needed.</param>
        public AuthenticateResult(IIdentity identity, IDictionary<string, string> extra, IDictionary<string, object> description)
        {
            if (identity != null)
            {
                Identity = identity as ClaimsIdentity ?? new ClaimsIdentity(identity);
            }
            Properties = new AuthenticationProperties(extra);
            Description = new AuthenticationDescription(description);
        }

        /// <summary>
        /// Contains the claims that were authenticated by the given AuthenticationType. If the authentication
        /// type was not successful the Identity property will be null.
        /// </summary>
        public ClaimsIdentity Identity { get; private set; }

        /// <summary>
        /// Contains extra values that were provided with the original SignIn call.
        /// </summary>
        public AuthenticationProperties Properties { get; private set; }

        /// <summary>
        /// Contains description properties for the middleware authentication type in general. Does not
        /// vary per request.
        /// </summary>
        public AuthenticationDescription Description { get; private set; }
    }
}

#endif
