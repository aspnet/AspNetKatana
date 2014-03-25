// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Owin.Security.Notifications
{
    public class SecurityTokenValidatedNotification<TOptions> : BaseNotification<TOptions>
    {
        public SecurityTokenValidatedNotification(IOwinContext context, TOptions options)
            : base(context, options)
        {
        }

        public AuthenticationTicket AuthenticationTicket { get; set; }
    }
}