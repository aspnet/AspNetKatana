// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Security.Claims;
using Microsoft.Owin.Security.OAuth.Messages;
using Microsoft.Owin.Security.Provider;

namespace Microsoft.Owin.Security.OAuth
{
    public class OAuthTokenEndpointContext : EndpointContext
    {
        public OAuthTokenEndpointContext(
            IOwinContext context,
            AuthenticationTicket ticket,
            TokenEndpointRequest tokenEndpointRequest) : base(context)
        {
            if (ticket == null)
            {
                throw new ArgumentNullException("ticket");
            }

            Identity = ticket.Identity;
            Extra = ticket.Extra;
            TokenEndpointRequest = tokenEndpointRequest;
            TokenIssued = Identity != null;
        }

        public ClaimsIdentity Identity { get; private set; }
        public AuthenticationExtra Extra { get; private set; }

        public TokenEndpointRequest TokenEndpointRequest { get; set; }

        public bool TokenIssued { get; private set; }

        public void Issue(ClaimsIdentity identity, AuthenticationExtra extra)
        {
            Identity = identity;
            Extra = extra;
            TokenIssued = true;
        }
    }
}
