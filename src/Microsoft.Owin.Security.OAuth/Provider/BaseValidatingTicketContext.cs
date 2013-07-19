// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Security.Claims;

namespace Microsoft.Owin.Security.OAuth
{
    public abstract class BaseValidatingTicketContext<TOptions> : BaseValidatingContext<TOptions>
    {
        protected BaseValidatingTicketContext(
            IOwinContext context,
            TOptions options,
            AuthenticationTicket ticket)
            : base(context, options)
        {
            Ticket = ticket;
            if (ticket != null && ticket.Identity != null)
            {
                // ticket with validated identity is initially acceptable
                Validated();
            }
        }

        public AuthenticationTicket Ticket { get; private set; }

        public void Validated(AuthenticationTicket ticket)
        {
            Ticket = ticket;
            Validated();
        }

        public void Validated(ClaimsIdentity identity)
        {
            AuthenticationProperties properties = Ticket != null ? Ticket.Properties : new AuthenticationProperties();
            Validated(new AuthenticationTicket(identity, properties));
        }
    }
}
