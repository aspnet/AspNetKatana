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

        public NotificationResultState State { get; set; }

        public bool Skipped
        {
            get { return State == NotificationResultState.Skipped; }
        }

        /// <summary>
        /// Discontinue processing the request in the current middleware and pass control to the next one.
        /// </summary>
        public void SkipToNextMiddleware()
        {
            State = NotificationResultState.Skipped;
        }
    }
}