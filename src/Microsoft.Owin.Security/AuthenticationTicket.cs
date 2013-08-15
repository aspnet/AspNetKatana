// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Security.Claims;

namespace Microsoft.Owin.Security
{
    public class AuthenticationTicket
    {
        public AuthenticationTicket(ClaimsIdentity identity, AuthenticationProperties properties)
        {
            Identity = identity;
            Properties = properties;
        }

        public ClaimsIdentity Identity { get; private set; }
        public AuthenticationProperties Properties { get; private set; }
    }
}
