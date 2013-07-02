// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Owin.Security.Cookies
{
    /// <summary>
    /// Default values related to cookie-based authentication middleware
    /// </summary>
    public static class CookiesAuthenticationDefaults
    {
        /// <summary>
        /// The default value used for CookiesAuthenticationOptions.AuthenticationType
        /// </summary>
        public const string AuthenticationType = "Cookies";

        /// <summary>
        /// The AuthenticationType used specifically by the UseApplicationSignInCookie extension method.
        /// </summary>
        public const string ApplicationAuthenticationType = "Application";

        /// <summary>
        /// The AuthenticationType used specifically by the UseExternalSignInCookie extension method.
        /// </summary>
        public const string ExternalAuthenticationType = "External";

        /// <summary>
        /// The prefix used to provide a default CookiesAuthenticationOptions.CookieName
        /// </summary>
        public const string CookiePrefix = ".AspNet.";

        /// <summary>
        /// The default value used by UseApplicationSignInCookie for the
        /// CookiesAuthenticationOptions.LoginPath
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Login", Justification = "By design")]
        public const string LoginPath = "/Account/Login";

        /// <summary>
        /// The default value used by UseApplicationSignInCookie for the
        /// CookiesAuthenticationOptions.LogoutPath
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Logout", Justification = "By design")]
        public const string LogoutPath = "/Account/Logout";

        /// <summary>
        /// The default value of the CookiesAuthenticationOptions.ReturnUrlParameter
        /// </summary>
        public const string ReturnUrlParameter = "ReturnUrl";
    }
}
