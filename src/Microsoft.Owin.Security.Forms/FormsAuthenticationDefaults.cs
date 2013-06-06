// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Owin.Security.Forms
{
    public static class FormsAuthenticationDefaults
    {
        public const string AuthenticationType = "Forms";
        public const string ApplicationAuthenticationType = "Application";
        public const string ExternalAuthenticationType = "External";

        public const string CookiePrefix = ".AspNet.";
        public const string LoginPath = "/Account/Login";
        public const string LogoutPath = "/Account/Logout";
        public const string ReturnUrlParameter = "ReturnUrl";
    }
}
