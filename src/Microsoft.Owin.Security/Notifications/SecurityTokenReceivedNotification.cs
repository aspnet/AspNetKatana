// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Owin.Security.Provider;

namespace Microsoft.Owin.Security.Notifications
{
    public class SecurityTokenReceivedNotification<TOptions> : BaseContext<TOptions>
    {
        public SecurityTokenReceivedNotification(IOwinContext context, TOptions options)
            : base(context, options)
        {
        }

        public bool Cancel { get; set; }
        public string SecurityToken { get; set; }
    }
}