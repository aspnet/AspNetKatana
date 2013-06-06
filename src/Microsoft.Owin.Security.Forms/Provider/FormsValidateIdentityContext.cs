// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Security.Claims;
using System.Security.Principal;

namespace Microsoft.Owin.Security.Forms
{
    public class FormsValidateIdentityContext
    {
        public FormsValidateIdentityContext(AuthenticationTicket ticket)
        {
            if (ticket == null)
            {
                throw new ArgumentNullException("ticket");
            }

            Identity = ticket.Identity;
            Extra = ticket.Extra;
        }

        public ClaimsIdentity Identity { get; private set; }
        public AuthenticationExtra Extra { get; private set; }

        public void ReplaceIdentity(IIdentity identity)
        {
            Identity = new ClaimsIdentity(identity);
        }

        public void RejectIdentity()
        {
            Identity = null;
        }
    }
}
