using System;
using System.Security.Claims;

namespace Microsoft.Owin.Security.Infrastructure
{
    public class AuthenticationTicketProviderContext
    {
        private AuthenticationTicket _ticket;
        private string _ticketData;

        public AuthenticationTicketProviderContext()
        {
        }

        public AuthenticationTicketProviderContext(ISecureDataHandler<AuthenticationTicket> secureDataHandler)
        {
            if (secureDataHandler == null)
            {
                throw new ArgumentNullException("secureDataHandler");
            }
            SecureDataHandler = secureDataHandler;
        }

        public ISecureDataHandler<AuthenticationTicket> SecureDataHandler { get; private set; }

        public string TokenValue { get; set; }

        public string ProtectedData { get { return TryEnsureTicketData() ? _ticketData : null; } }

        public AuthenticationTicket Ticket { get { return TryEnsureTicket() ? _ticket : null; } }
        public ClaimsIdentity Identity { get { return TryEnsureTicket() ? _ticket.Identity : null; } }
        public AuthenticationExtra Extra { get { return TryEnsureTicket() ? _ticket.Extra : null; } }

        public void SetProtectedData(string protectedData)
        {
            if (protectedData == null)
            {
                throw new ArgumentNullException("protectedData");
            }
            _ticket = null;
            _ticketData = protectedData;
        }

        public void SetTicket(AuthenticationTicket ticket)
        {
            if (ticket == null)
            {
                throw new ArgumentNullException("ticket");
            }
            _ticket = ticket;
            _ticketData = null;
        }

        public void SetTicket(ClaimsIdentity identity, AuthenticationExtra extra)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }
            if (extra == null)
            {
                throw new ArgumentNullException("extra");
            }
            _ticket = new AuthenticationTicket(identity, extra);
            _ticketData = null;
        }

        private bool TryEnsureTicket()
        {
            if (!HasTicket() && HasTicketData() && SecureDataHandler != null)
            {
                _ticket = SecureDataHandler.Unprotect(_ticketData);
            }
            return HasTicket();
        }

        private bool TryEnsureTicketData()
        {
            if (!HasTicketData() && HasTicket() && SecureDataHandler != null)
            {
                _ticketData = SecureDataHandler.Protect(_ticket);
            }
            return HasTicketData();
        }

        private bool HasTicketData()
        {
            return !string.IsNullOrEmpty(_ticketData);
        }

        private bool HasTicket()
        {
            return _ticket != null;
        }
    }
}