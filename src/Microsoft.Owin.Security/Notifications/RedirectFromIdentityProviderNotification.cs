// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Owin.Security.Provider;

namespace Microsoft.Owin.Security.Notifications
{
    public class RedirectFromIdentityProviderNotification<TOptions> : BaseContext<TOptions>
    {
        public RedirectFromIdentityProviderNotification(IOwinContext context, TOptions options)
            : base(context, options)
        {
        }

        public AuthenticationTicket AuthenticationTicket { get; set; }

        public string SignInAsAuthenticationType { get; set; }

        public bool Cancel { get; set; }

        public bool IsRequestCompleted { get; set; }
    }
}
