// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Owin.Security.Facebook
{
    internal static class Constants
    {
        public const string DefaultAuthenticationType = "Facebook";

        // https://developers.facebook.com/docs/facebook-login/manually-build-a-login-flow#login
        internal const string AuthorizationEndpoint = "https://www.facebook.com/v10.0/dialog/oauth";
        internal const string TokenEndpoint = "https://graph.facebook.com/v10.0/oauth/access_token";
        internal const string UserInformationEndpoint = "https://graph.facebook.com/v10.0/me";
    }
}
