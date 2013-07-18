// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Owin.Security.OAuth
{
    public class OAuthGrantResourceOwnerCredentialsContext : BaseValidatingTicketContext
    {
        public OAuthGrantResourceOwnerCredentialsContext(
            IOwinContext context,
            OAuthAuthorizationServerOptions options,
            string clientId,
            string userName,
            string password,
            string scope)
            : base(context, options, null)
        {
            ClientId = clientId;
            UserName = userName;
            Password = password;
            Scope = scope;
        }

        public string ClientId { get; private set; }
        public string UserName { get; private set; }
        public string Password { get; private set; }
        public string Scope { get; private set; }
    }
}
