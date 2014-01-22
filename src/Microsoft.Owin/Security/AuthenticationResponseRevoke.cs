// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Owin.Security
{
    /// <summary>
    /// Exposes the security.SignOut environment value as a strong type.
    /// </summary>
    public class AuthenticationResponseRevoke
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationResponseRevoke"/> class
        /// </summary>
        /// <param name="authenticationTypes"></param>
        public AuthenticationResponseRevoke(string[] authenticationTypes)
        {
            AuthenticationTypes = authenticationTypes;
        }

        /// <summary>
        /// List of the authentication types that should be revoked on sign out.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "By design")]
        public string[] AuthenticationTypes { get; private set; }
    }
}
