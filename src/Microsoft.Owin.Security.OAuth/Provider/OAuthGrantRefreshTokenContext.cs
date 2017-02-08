// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Owin.Security.OAuth
{
    /// <summary>
    /// Provides context information used when granting an OAuth refresh token.
    /// </summary>
    public class OAuthGrantRefreshTokenContext : BaseValidatingTicketContext<OAuthAuthorizationServerOptions>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthGrantRefreshTokenContext"/> class
        /// </summary>
        /// <param name="context"></param>
        /// <param name="options"></param>
        /// <param name="ticket"></param>
        /// <param name="clientId"></param>
        public OAuthGrantRefreshTokenContext(
            IOwinContext context,
            OAuthAuthorizationServerOptions options,
            AuthenticationTicket ticket,
            string clientId) : base(context, options, ticket)
        {
            ClientId = clientId;
        }

        /// <summary>
        /// The OAuth client id.
        /// </summary>
        public string ClientId { get; private set; }
    }
}
