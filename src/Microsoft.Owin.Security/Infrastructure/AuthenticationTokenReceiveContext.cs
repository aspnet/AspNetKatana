// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;

namespace Microsoft.Owin.Security.Infrastructure
{
    public class AuthenticationTokenReceiveContext
    {
        private readonly ISecureDataHandler<AuthenticationTicket> _secureDataHandler;

        public AuthenticationTokenReceiveContext(
            ISecureDataHandler<AuthenticationTicket> secureDataHandler,
            string token)
        {
            if (secureDataHandler == null)
            {
                throw new ArgumentNullException("secureDataHandler");
            }
            if (token == null)
            {
                throw new ArgumentNullException("token");
            }
            _secureDataHandler = secureDataHandler;
            Token = token;
        }

        public string Token { get; protected set; }

        public AuthenticationTicket Ticket { get; protected set; }

        public void DeserializeTicket(string protectedData)
        {
            Ticket = _secureDataHandler.Unprotect(protectedData);
        }

        public void SetTicket(AuthenticationTicket ticket)
        {
            if (ticket == null)
            {
                throw new ArgumentNullException("ticket");
            }
            Ticket = ticket;
        }
    }
}
