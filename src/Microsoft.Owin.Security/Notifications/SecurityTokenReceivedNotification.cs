// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Owin.Security.Notifications
{
    public class SecurityTokenReceivedNotification<TOptions> : BaseNotification<TOptions>
    {
        public SecurityTokenReceivedNotification(IOwinContext context, TOptions options)
            : base(context, options)
        {
        }

        public string SecurityToken { get; set; }
    }
}