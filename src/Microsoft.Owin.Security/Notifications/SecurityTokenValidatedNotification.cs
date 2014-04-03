// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Owin.Security.Notifications
{
    public class SecurityTokenValidatedNotification<TMessage, TOptions> : BaseNotification<TOptions>
    {
        public SecurityTokenValidatedNotification(IOwinContext context, TOptions options)
            : base(context, options)
        {
        }

        /// <summary>
        /// Gets or set the <see cref="AuthenticationTicket"/>
        /// </summary>
        public AuthenticationTicket AuthenticationTicket { get; set; }

        /// <summary>
        /// Gets or sets the Protocol message
        /// </summary>
        public TMessage ProtocolMessage { get; set; }
    }
}