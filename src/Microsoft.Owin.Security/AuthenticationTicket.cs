// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Claims;

namespace Microsoft.Owin.Security
{
    /// <summary>
    /// Contains user identity information as well as additional authentication state.
    /// </summary>
    public class AuthenticationTicket
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationTicket"/> class
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="properties"></param>
        public AuthenticationTicket(ClaimsIdentity identity, AuthenticationProperties properties)
        {
            Identity = identity;
            Properties = properties ?? new AuthenticationProperties();
        }

        /// <summary>
        /// Gets the authenticated user identity.
        /// </summary>
        public ClaimsIdentity Identity { get; private set; }

        /// <summary>
        /// Additional state values for the authentication session.
        /// </summary>
        public AuthenticationProperties Properties { get; private set; }
    }
}
