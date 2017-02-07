// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Owin.Security.MicrosoftAccount
{
    internal static class Constants
    {
        internal const string DefaultAuthenticationType = "Microsoft";

        internal const string AuthorizationEndpoint = "https://login.microsoftonline.com/common/oauth2/v2.0/authorize";
        internal const string TokenEndpoint = "https://login.microsoftonline.com/common/oauth2/v2.0/token";
        internal const string UserInformationEndpoint = "https://graph.microsoft.com/v1.0/me";
    }
}
