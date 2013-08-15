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
        /// Access the Authentication middleware functionality available on the current request.
        /// </summary>
        IAuthenticationManager Authentication { get; }
    }
}

#else

using ResharperCodeFormattingWorkaround = System.Object;

#endif
