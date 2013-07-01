// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;

namespace Microsoft.Owin.Security.Infrastructure
{
    public class AuthenticationTokenCreateContext
    {
        private readonly ISecureDataHandler<AuthenticationTicket> _secureDataHandler;

        public AuthenticationTokenCreateContext(
            ISecureDataHandler<AuthenticationTicket> secureDataHandler,
            AuthenticationTicket ticket)
        {
            if (secureDataHandler == null)
            {
                throw new ArgumentNullException("secureDataHandler");
            }
            if (ticket == null)
            {
                throw new ArgumentNullException("ticket");
            }
            _secureDataHandler = secureDataHandler;
            Ticket = ticket;
        }

        public string Token { get; protected set; }

        public AuthenticationTicket Ticket { get; protected set; }

        public string SerializeTicket()
        {
            return _secureDataHandler.Protect(Ticket);
        }

        public void SetToken(string tokenValue)
        {
            if (tokenValue == null)
            {
                throw new ArgumentNullException("tokenValue");
            }
            Token = tokenValue;
        }
    }
}
