// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if !NET40

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Owin.Security
{
    /// <summary>
    /// 
    /// </summary>
    public class AuthenticationResponseRevoke
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="authenticationTypes"></param>
        public AuthenticationResponseRevoke(string[] authenticationTypes)
        {
            AuthenticationTypes = authenticationTypes;
        }

        /// <summary>
        /// 
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "By design")]
        public string[] AuthenticationTypes { get; private set; }
    }
}

#else

using ResharperCodeFormattingWorkaround = System.Object;

#endif
