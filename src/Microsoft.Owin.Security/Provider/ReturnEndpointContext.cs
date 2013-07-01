// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace Microsoft.Owin.Security.Provider
{
    public abstract class ReturnEndpointContext : EndpointContext
    {
        protected ReturnEndpointContext(
            IDictionary<string, object> environment,
            AuthenticationTicket ticket,
            IDictionary<string, string> errorDetails)
            : base(environment)
        {
            ErrorDetails = errorDetails;
            if (ticket != null)
            {
                Identity = ticket.Identity;
                Extra = ticket.Extra;
            }
        }

        public ClaimsIdentity Identity { get; set; }
        public AuthenticationExtra Extra { get; set; }
        public IDictionary<string, string> ErrorDetails { get; private set; }

        public string SignInAsAuthenticationType { get; set; }

        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "By design")]
        public string RedirectUri { get; set; }
    }
}
