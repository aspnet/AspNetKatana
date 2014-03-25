// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Owin.Security.Notifications
{
    public class MessageReceivedNotification<TMessage, TOptions> : BaseNotification<TOptions>
    {
        public MessageReceivedNotification(IOwinContext context, TOptions options)
            : base(context, options)
        {
        }

        public TMessage ProtocolMessage { get; set; }
    }
}