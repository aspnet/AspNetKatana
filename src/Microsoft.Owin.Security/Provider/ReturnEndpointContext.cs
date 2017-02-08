// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace Microsoft.Owin.Security.Provider
{
    public abstract class ReturnEndpointContext : EndpointContext
    {
        protected ReturnEndpointContext(
            IOwinContext context,
            AuthenticationTicket ticket)
            : base(context)
        {
            if (ticket != null)
            {
                Identity = ticket.Identity;
                Properties = ticket.Properties;
            }
        }

        public ClaimsIdentity Identity { get; set; }
        public AuthenticationProperties Properties { get; set; }

        public string SignInAsAuthenticationType { get; set; }

        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "By design")]
        public string RedirectUri { get; set; }
    }
}
