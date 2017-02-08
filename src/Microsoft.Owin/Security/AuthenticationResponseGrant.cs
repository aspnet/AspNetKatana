// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Security.Claims;

namespace Microsoft.Owin.Security
{
    /// <summary>
    /// Exposes the security.SignIn environment value as a strong type.
    /// </summary>
    public class AuthenticationResponseGrant
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationResponseGrant"/> class.
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
        /// Initializes a new instance of the <see cref="AuthenticationResponseGrant"/> class.
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
        /// The identity associated with the user sign in.
        /// </summary>
        public ClaimsIdentity Identity { get; private set; }

        /// <summary>
        /// The security principal associated with the user sign in.
        /// </summary>
        public ClaimsPrincipal Principal { get; private set; }

        /// <summary>
        /// Dictionary used to store state values about the authentication session.
        /// </summary>
        public AuthenticationProperties Properties { get; private set; }
    }
}
