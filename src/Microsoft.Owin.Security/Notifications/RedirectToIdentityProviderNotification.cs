// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Owin.Security.Provider;

namespace Microsoft.Owin.Security.Notifications
{
    public class RedirectToIdentityProviderNotification<TMessage, TOptions> : BaseContext<TOptions>
    {
        public RedirectToIdentityProviderNotification(IOwinContext context, TOptions options)
            : base(context, options)
        {
        }

        public TMessage ProtocolMessage { get; set; }

        public bool Canceled { get; private set; }

        public void Cancel()
        {
            Canceled = true;
        }
    }
}