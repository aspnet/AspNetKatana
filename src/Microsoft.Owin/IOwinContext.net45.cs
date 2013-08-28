// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if !NET40

using Microsoft.Owin.Security;

namespace Microsoft.Owin
{
    /// <summary>
    /// This wraps OWIN environment dictionary and provides strongly typed accessors.
    /// </summary>
    public partial interface IOwinContext
    {
        /// <summary>
        /// Gets the Authentication middleware functionality available on the current request.
        /// </summary>
        /// <returns>The authentication middleware functionality available on the current request.</returns>
        IAuthenticationManager Authentication { get; }
    }
}

#else

using ResharperCodeFormattingWorkaround = System.Object;

#endif
