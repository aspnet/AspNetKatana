// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Security.Claims;
using System.Security.Principal;

namespace Microsoft.Owin.Security.Cookies
{
    /// <summary>
    /// Context object passed to the ICookiesAuthenticationProvider method ValidateIdentity.
    /// </summary>
    public class CookiesValidateIdentityContext
    {
        /// <summary>
        /// Creates a new instance of the context object.
        /// </summary>
        /// <param name="ticket">Contains the initial values for identity and extra data</param>
        public CookiesValidateIdentityContext(AuthenticationTicket ticket)
        {
            if (ticket == null)
            {
                throw new ArgumentNullException("ticket");
            }

            Identity = ticket.Identity;
            Extra = ticket.Extra;
        }

        /// <summary>
        /// Contains the claims identity arriving with the request. May be altered to change the 
        /// details of the authenticated user.
        /// </summary>
        public ClaimsIdentity Identity { get; private set; }

        /// <summary>
        /// Contains the extra metadata arriving with the request ticket. May be altered.
        /// </summary>
        public AuthenticationExtra Extra { get; private set; }

        /// <summary>
        /// Called to replace the claims identity. The supplied identity will replace the value of the 
        /// Identity property, which determines the identity of the authenticated request.
        /// </summary>
        /// <param name="identity">The identity used as the replacement</param>
        public void ReplaceIdentity(IIdentity identity)
        {
            Identity = new ClaimsIdentity(identity);
        }

        /// <summary>
        /// Called to reject the incoming identity. This may be done if the application has determined the
        /// account is no longer active, and the request should be treated as if it was anonymous.
        /// </summary>
        public void RejectIdentity()
        {
            Identity = null;
        }
    }
}
