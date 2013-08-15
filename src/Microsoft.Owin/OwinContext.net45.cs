// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if !NET40

using Microsoft.Owin.Security;

namespace Microsoft.Owin
{
    /// <summary>
    /// This wraps OWIN environment dictionary and provides strongly typed accessors.
    /// </summary>
    public partial class OwinContext : IOwinContext
    {
        /// <summary>
        /// Access the Authentication middleware functionality available on the current request.
        /// </summary>
        public IAuthenticationManager Authentication
        {
            get { return new AuthenticationManager(this); }
        }
    }
}

#else

using ResharperCodeFormattingWorkaround = System.Object;

#endif
