// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Owin.Security.Provider;

namespace Microsoft.Owin.Security.Notifications
{
    public class SecurityTokenValidatedNotification<TOptions> : BaseContext<TOptions>
    {
        public SecurityTokenValidatedNotification(IOwinContext context, TOptions options)
            : base(context, options)
        {
        }

        public AuthenticationTicket AuthenticationTicket { get; set; }
        public bool Cancel { get; set; }
    }
}