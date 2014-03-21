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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "by design")]
        public string CurrentUri { get; set; }
        public bool Cancel { get; set; }
        public TMessage ProtocolMessage { get; set; }
    }
}