// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if !NET40

using System;
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

#else

using ResharperCodeFormattingWorkaround = System.Object;

#endif
