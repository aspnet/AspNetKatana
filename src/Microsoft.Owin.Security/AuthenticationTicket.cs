// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Security.Claims;

namespace Microsoft.Owin.Security
{
    public class AuthenticationTicket
    {
        public AuthenticationTicket(ClaimsIdentity identity, AuthenticationExtra extra)
        {
            Identity = identity;
            Extra = extra;
        }

        public AuthenticationTicket(ClaimsIdentity identity, IDictionary<string, string> extra)
        {
            Identity = identity;
            Extra = new AuthenticationExtra(extra);
        }

        public ClaimsIdentity Identity { get; private set; }
        public AuthenticationExtra Extra { get; private set; }
    }
}
