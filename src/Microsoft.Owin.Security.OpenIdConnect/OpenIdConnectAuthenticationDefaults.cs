// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Owin.Security.OpenIdConnect
{
    /// <summary>
    /// Default values related to OpenIdConnect authentication middleware
    /// </summary>
    public static class OpenIdConnectAuthenticationDefaults
    {
        /// <summary>
        /// The default value used for OpenIdConnectAuthenticationOptions.AuthenticationType
        /// </summary>
        public const string AuthenticationType = "OpenIdConnect";

        /// <summary>
        /// The prefix used to provide a default OpenIdConnectAuthenticationOptions.CookieName
        /// </summary>
        public const string CookiePrefix = "OpenIdConnect.";

        /// <summary>
        /// The prefix used to for the a nonce in the cookie the nonce
        /// </summary>
        public const string Nonce = "nonce.";

        public const string CodeKey = "OpenIdConnect.Code";

        public const string RedirectUriUsedForCodeKey = "OpenIdConnect.Code.RedirectUri";

        internal const string AuthenticationPropertiesKey = "OpenIdConnect.AuthenticationProperties";
    }
}
