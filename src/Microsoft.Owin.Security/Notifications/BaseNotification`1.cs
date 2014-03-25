// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Owin.Security.Provider;

namespace Microsoft.Owin.Security.Notifications
{
    public class BaseNotification<TOptions> : BaseContext<TOptions>
    {
        protected BaseNotification(IOwinContext context, TOptions options)
            : base(context, options)
        {
        }

        public bool Redirected { get; protected set; }

        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "By design")]
        public string RedirectUri { get; private set; }

        public bool Canceled { get; protected set; }

        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#", Justification = "By design")]
        public void Redirect(string redirectUri)
        {
            Redirected = true;
            RedirectUri = redirectUri;
            Canceled = false;
        }

        public void Cancel()
        {
            Canceled = true;
            Redirected = false;
        }
    }
}
