// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Owin.Security.OAuth
{
    /// <summary>
    /// Default values used by authorization server and bearer authentication.
    /// </summary>
    public static class OAuthDefaults
    {
        /// <summary>
        /// Default value for AuthenticationType property in the OAuthBearerAuthenticationOptions and
        /// OAuthAuthorizationServerOptions.
        /// </summary>
        public const string AuthenticationType = "Bearer";
    }
}
