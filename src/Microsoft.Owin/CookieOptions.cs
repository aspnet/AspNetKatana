// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;

namespace Microsoft.Owin
{
    /// <summary>
    /// Options used to create a new cookie.
    /// </summary>
    public class CookieOptions
    {
        /// <summary>
        /// Creates a default cookie with a path of '/'.
        /// </summary>
        public CookieOptions()
        {
            Path = "/";
        }

        /// <summary>
        /// The cookie domain.
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// The cookie path.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// The cookie expiration date.
        /// </summary>
        public DateTime? Expires { get; set; }

        /// <summary>
        /// The cookie security requirement.
        /// </summary>
        public bool Secure { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool HttpOnly { get; set; }
    }
}
