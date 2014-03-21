// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Owin.Security.Provider;

namespace Microsoft.Owin.Security.Notifications
{
    public class MessageReceivedNotification<TMessage, TOptions> : BaseContext<TOptions>
    {
        public MessageReceivedNotification(IOwinContext context, TOptions options)
            : base(context, options)
        {
        }

        public bool Cancel { get; set; }
        public TMessage ProtocolMessage { get; set; }
    }
}