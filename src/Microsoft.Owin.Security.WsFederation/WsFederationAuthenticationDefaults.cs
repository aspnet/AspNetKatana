// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Owin.Security.WsFederation
{
    /// <summary>
    /// Default values related to WsFederation authentication middleware
    /// </summary>
    public static class WsFederationAuthenticationDefaults
    {
        /// <summary>
        /// The default value used for WsFederationAuthenticationOptions.AuthenticationType
        /// </summary>
        public const string AuthenticationType = "Federation";

        /// <summary>
        /// The prefix used to provide a default WsFederationAuthenticationOptions.CookieName
        /// </summary>
        public const string CookiePrefix = "WsFederation.";

        /// <summary>
        /// The prefix used to provide a default WsFederationAuthenticationOptions.CookieName
        /// </summary>
        public const string CookieName = "WsFederationAuth";

        public const string Caption = "WsFederation";

    }
}
