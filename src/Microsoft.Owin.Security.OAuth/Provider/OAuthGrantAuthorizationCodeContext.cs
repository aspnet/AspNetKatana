// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Owin.Security.OAuth
{
    /// <summary>
    /// Provides context information when handling an OAuth authorization code grant.
    /// </summary>
    public class OAuthGrantAuthorizationCodeContext : BaseValidatingTicketContext<OAuthAuthorizationServerOptions>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthGrantAuthorizationCodeContext"/> class
        /// </summary>
        /// <param name="context"></param>
        /// <param name="options"></param>
        /// <param name="ticket"></param>
        public OAuthGrantAuthorizationCodeContext(
            IOwinContext context,
            OAuthAuthorizationServerOptions options,
            AuthenticationTicket ticket) : base(context, options, ticket)
        {
        }
    }
}
