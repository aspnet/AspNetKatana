// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Owin.Security.Google
{
    internal static class Constants
    {
        internal const string DefaultAuthenticationType = "Google";

        // https://developers.google.com/identity/protocols/oauth2/web-server#httprest
        internal const string AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
        internal const string TokenEndpoint = "https://oauth2.googleapis.com/token";
        internal const string UserInformationEndpoint = "https://www.googleapis.com/oauth2/v2/userinfo";
    }
}
