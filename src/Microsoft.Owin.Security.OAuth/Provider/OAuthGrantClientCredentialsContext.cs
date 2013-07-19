// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Owin.Security.OAuth
{
    public class OAuthGrantClientCredentialsContext : BaseValidatingTicketContext<OAuthAuthorizationServerOptions>
    {
        public OAuthGrantClientCredentialsContext(
            IOwinContext context,
            OAuthAuthorizationServerOptions options,
            string clientId,
            string scope)
            : base(context, options, null)
        {
            ClientId = clientId;
            Scope = scope;
        }

        public string ClientId { get; private set; }
        public string Scope { get; private set; }
    }
}
