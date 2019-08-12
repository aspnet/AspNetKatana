// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Owin.Security.OpenIdConnect;

namespace Microsoft.Owin.Security.Notifications
{
    /// <summary>
    /// This Notification can be used to be informed when an 'AuthorizationCode' is redeemed for tokens at the token endpoint.
    /// </summary>
    public class TokenResponseReceivedNotification : BaseNotification<OpenIdConnectAuthenticationOptions>
    {
        /// <summary>
        /// Creates a <see cref="TokenResponseReceivedNotification"/>
        /// </summary>
        public TokenResponseReceivedNotification(IOwinContext context, OpenIdConnectAuthenticationOptions options)
            : base(context, options)
        {
        }

        /// <summary>
        /// Gets or sets the <see cref="OpenIdConnectMessage"/> that contains the code redeemed for tokens at the token endpoint.
        /// </summary>
        public OpenIdConnectMessage ProtocolMessage { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="OpenIdConnectMessage"/> that contains the tokens received after redeeming the code at the token endpoint.
        /// </summary>
        public OpenIdConnectMessage TokenEndpointResponse { get; set; }
    }
}
