// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Owin.Security.OAuth
{
    public class OAuthGrantAuthorizationCodeContext : BaseValidatingTicketContext
    {
        public OAuthGrantAuthorizationCodeContext(
            IOwinContext context,
            OAuthAuthorizationServerOptions options,
            AuthenticationTicket ticket) : base(context, options, ticket)
        {
        }
    }
}
