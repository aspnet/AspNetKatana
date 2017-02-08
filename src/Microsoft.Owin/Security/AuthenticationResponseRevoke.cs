// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Owin.Security
{
    /// <summary>
    /// Exposes the security.SignOut and security.SignOutProperties environment values as a strong type.
    /// </summary>
    public class AuthenticationResponseRevoke
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationResponseRevoke"/> class
        /// </summary>
        /// <param name="authenticationTypes"></param>
        public AuthenticationResponseRevoke(string[] authenticationTypes)
            : this(authenticationTypes, new AuthenticationProperties())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationResponseRevoke"/> class
        /// </summary>
        /// <param name="authenticationTypes"></param>
        /// <param name="properties"></param>
        public AuthenticationResponseRevoke(string[] authenticationTypes, AuthenticationProperties properties)
        {
            AuthenticationTypes = authenticationTypes;
            Properties = properties;
        }

        /// <summary>
        /// List of the authentication types that should be revoked on sign out.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "By design")]
        public string[] AuthenticationTypes { get; private set; }

        /// <summary>
        /// Dictionary used to store state values about the authentication session.
        /// </summary>
        public AuthenticationProperties Properties { get; private set; }
    }
}
