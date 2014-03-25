// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;

namespace Microsoft.Owin.Security.Notifications
{
    public class AuthenticationFailedNotification<TMessage, TOptions> : BaseNotification<TOptions>
    {
        public AuthenticationFailedNotification(IOwinContext context, TOptions options)
            : base(context, options)
        {
        }

        public TMessage ProtocolMessage { get; set; }

        public Exception Exception { get; set; }
    }
}