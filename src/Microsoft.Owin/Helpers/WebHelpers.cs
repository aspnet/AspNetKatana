// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if !NET40
using Microsoft.Owin.Infrastructure;

namespace Microsoft.Owin.Helpers
{
    /// <summary>
    /// Request processing helpers.
    /// </summary>
    public static class WebHelpers
    {
        /// <summary>
        /// Parses an HTTP form body.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static IFormCollection ParseForm(string text)
        {
            return OwinHelpers.GetForm(text);
        }
    }
}

#else

using ResharperCodeFormattingWorkaround = System.Object;

#endif
