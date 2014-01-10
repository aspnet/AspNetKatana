// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Owin.Security.OAuth
{
    /// <summary>
    /// Contains the authentication ticket data from an OAuth bearer token.
    /// </summary>
    public class OAuthValidateIdentityContext : BaseValidatingTicketContext<OAuthBearerAuthenticationOptions>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthValidateIdentityContext"/> class
        /// </summary>
        /// <param name="context"></param>
        /// <param name="options"></param>
        /// <param name="ticket"></param>
        public OAuthValidateIdentityContext(
            IOwinContext context,
            OAuthBearerAuthenticationOptions options,
            AuthenticationTicket ticket) : base(context, options, ticket)
        {
        }
    }
}
