// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Owin.Security.Cookies
{
    /// <summary>
    /// Default values related to cookie-based authentication middleware
    /// </summary>
    public static class CookieAuthenticationDefaults
    {
        /// <summary>
        /// The default value used for CookieAuthenticationOptions.AuthenticationType
        /// </summary>
        public const string AuthenticationType = "Cookies";

        /// <summary>
        /// The prefix used to provide a default CookieAuthenticationOptions.CookieName
        /// </summary>
        public const string CookiePrefix = ".AspNet.";

        /// <summary>
        /// The default value of the CookieAuthenticationOptions.ReturnUrlParameter
        /// </summary>
        public const string ReturnUrlParameter = "ReturnUrl";
    }
}
