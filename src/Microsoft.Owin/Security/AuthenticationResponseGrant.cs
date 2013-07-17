// <copyright file="AuthenticationResponseGrant.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Linq;
using System.Security.Claims;

namespace Microsoft.Owin.Security
{
    /// <summary>
    /// 
    /// </summary>
    public class AuthenticationResponseGrant
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="properties"></param>
        public AuthenticationResponseGrant(ClaimsIdentity identity, AuthenticationProperties properties)
        {
            Principal = new ClaimsPrincipal(identity);
            Identity = identity;
            Properties = properties;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="principal"></param>
        /// <param name="properties"></param>
        public AuthenticationResponseGrant(ClaimsPrincipal principal, AuthenticationProperties properties)
        {
            if (principal == null)
            {
                throw new ArgumentNullException("principal");
            }

            Principal = principal;
            Identity = principal.Identities.FirstOrDefault();
            Properties = properties;
        }

        /// <summary>
        /// 
        /// </summary>
        public ClaimsIdentity Identity { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public ClaimsPrincipal Principal { get; private set; }
        
        /// <summary>
        /// 
        /// </summary>
        public AuthenticationProperties Properties { get; private set; }
    }
}

#endif
