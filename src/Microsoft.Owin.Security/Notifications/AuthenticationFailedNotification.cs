// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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