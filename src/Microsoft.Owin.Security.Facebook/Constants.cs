// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Owin.Security.Facebook
{
    internal static class Constants
    {
        public const string DefaultAuthenticationType = "Facebook";

        internal const string AuthorizationEndpoint = "https://www.facebook.com/dialog/oauth";
        internal const string TokenEndpoint = "https://graph.facebook.com/oauth/access_token";
        internal const string UserInformationEndpoint = "https://graph.facebook.com/me";
    }
}
