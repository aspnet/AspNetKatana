// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Owin.Infrastructure;

namespace Microsoft.Owin.Helpers
{
    /// <summary>
    /// Provides helper methods for processing requests.
    /// </summary>
    public static class WebHelpers
    {
        /// <summary>
        /// Parses an HTTP form body.
        /// </summary>
        /// <param name="text">The HTTP form body to parse.</param>
        /// <returns>The <see cref="T:Microsoft.Owin.IFormCollection" /> object containing the parsed HTTP form body.</returns>
        public static IFormCollection ParseForm(string text)
        {
            return OwinHelpers.GetForm(text);
        }
    }
}