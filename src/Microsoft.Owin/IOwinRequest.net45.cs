// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if !NET40

using System.Threading.Tasks;

namespace Microsoft.Owin
{
    /// <summary>
    /// This wraps OWIN environment dictionary and provides strongly typed accessors.
    /// </summary>
    public partial interface IOwinRequest
    {
        /// <summary>
        /// Parses the request body as a form
        /// </summary>
        Task<IFormCollection> ReadFormAsync();
    }
}

#else

using ResharperCodeFormattingWorkaround = System.Object;

#endif
