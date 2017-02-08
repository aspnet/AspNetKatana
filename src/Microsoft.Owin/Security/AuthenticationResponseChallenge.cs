// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Owin.Security
{
    /// <summary>
    /// Exposes the security.Challenge environment value as a strong type.
    /// </summary>
    public class AuthenticationResponseChallenge
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationResponseChallenge"/> class
        /// </summary>
        /// <param name="authenticationTypes"></param>
        /// <param name="properties"></param>
        public AuthenticationResponseChallenge(string[] authenticationTypes, AuthenticationProperties properties)
        {
            AuthenticationTypes = authenticationTypes;
            Properties = properties ?? new AuthenticationProperties();
        }

        /// <summary>
        /// List of the authentication types that should send a challenge in the response.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "By design")]
        public string[] AuthenticationTypes { get; private set; }

        /// <summary>
        /// Dictionary used to store state values about the authentication session.
        /// </summary>
        public AuthenticationProperties Properties { get; private set; }
    }
}
