// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if !NET40

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Owin.Security
{
    /// <summary>
    /// 
    /// </summary>
    public class AuthenticationResponseChallenge
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="authenticationTypes"></param>
        /// <param name="properties"></param>
        public AuthenticationResponseChallenge(string[] authenticationTypes, AuthenticationProperties properties)
        {
            AuthenticationTypes = authenticationTypes;
            Properties = properties;
        }

        /// <summary>
        /// 
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "By design")]
        public string[] AuthenticationTypes { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public AuthenticationProperties Properties { get; private set; }
    }
}

#else

using ResharperCodeFormattingWorkaround = System.Object;

#endif
