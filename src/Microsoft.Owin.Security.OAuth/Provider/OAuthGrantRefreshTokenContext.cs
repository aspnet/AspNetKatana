// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Owin.Security.OAuth
{
    public class OAuthGrantRefreshTokenContext : BaseValidatingTicketContext<OAuthAuthorizationServerOptions>
    {
        public OAuthGrantRefreshTokenContext(
            IOwinContext context,
            OAuthAuthorizationServerOptions options,
            AuthenticationTicket ticket,
            string clientId) : base(context, options, ticket)
        {
            ClientId = clientId;
        }

        public string ClientId { get; private set; }
    }
}
