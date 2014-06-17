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

        public bool HandledResponse
        {
            get { return State == NotificationResultState.HandledResponse; }
        }

        /// <summary>
        /// Discontinue all processing for this request and return to the client.
        /// The caller is responsible for generating the full response.
        /// </summary>
        public void HandleResponse()
        {
            State = NotificationResultState.HandledResponse;
        }
    }
}