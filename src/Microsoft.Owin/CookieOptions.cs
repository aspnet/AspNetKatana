// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
        /// Gets or sets the domain to associate the cookie with.
        /// </summary>
        /// <returns>The domain to associate the cookie with.</returns>
        public string Domain { get; set; }

        /// <summary>
        /// Gets or sets the cookie path.
        /// </summary>
        /// <returns>The cookie path.</returns>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the expiration date and time for the cookie.
        /// </summary>
        /// <returns>The expiration date and time for the cookie.</returns>
        public DateTime? Expires { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether to transmit the cookie using Secure Sockets Layer (SSL)�that is, over HTTPS only.
        /// </summary>
        /// <returns>true to transmit the cookie only over an SSL connection (HTTPS); otherwise, false.</returns>
        public bool Secure { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether a cookie is accessible by client-side script.
        /// </summary>
        /// <returns>false if a cookie is accessible by client-side script; otherwise, true.</returns>
        public bool HttpOnly { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates on which requests client should or should not send cookie back to the server.
        /// Set to null to do not include SameSite attribute at all.
        /// </summary>
        /// <returns>SameSite attribute value or null if attribute must not be set.</returns>
        public SameSiteMode? SameSite { get; set; }
    }
}
