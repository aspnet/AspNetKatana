// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Owin.Security.Provider;

namespace Microsoft.Owin.Security.Notifications
{
    public class BaseNotification<TOptions> : BaseContext<TOptions>
    {
        protected BaseNotification(IOwinContext context, TOptions options)
            : base(context, options)
        {
        }

        public NotificationResultState State { get; set; }

        public bool HandledResponse
        {
            get { return State == NotificationResultState.HandledResponse; }
        }

        public bool Skipped
        {
            get { return State == NotificationResultState.Skipped; }
        }

        /// <summary>
        /// Discontinue all processing for this request and return to the client.
        /// The caller is responsible for generating the full response.
        /// </summary>
        public void HandleResponse()
        {
            State = NotificationResultState.HandledResponse;
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
