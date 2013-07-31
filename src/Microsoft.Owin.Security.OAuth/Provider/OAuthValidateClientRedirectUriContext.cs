// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;

namespace Microsoft.Owin.Security.OAuth
{
    public class OAuthValidateClientRedirectUriContext : BaseValidatingClientContext
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "3#", Justification = "redirect_uri is a string parameter")]
        public OAuthValidateClientRedirectUriContext(
            IOwinContext context,
            OAuthAuthorizationServerOptions options,
            string clientId,
            string redirectUri)
            : base(context, options, clientId)
        {
            RedirectUri = redirectUri;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "redirect_uri is a string parameter")]
        public string RedirectUri { get; private set; }

        public override bool Validated()
        {
            if (String.IsNullOrEmpty(RedirectUri))
            {
                // Don't allow default validation when redirect_uri not provided with request
                return false;
            }
            return base.Validated();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#", Justification = "redirect_uri is a string parameter")]
        public bool Validated(string redirectUri)
        {
            if (redirectUri == null)
            {
                throw new ArgumentNullException("redirectUri");
            }

            if (!String.IsNullOrEmpty(RedirectUri) &&
                !String.Equals(RedirectUri, redirectUri, StringComparison.Ordinal))
            {
                // Don't allow validation to alter redirect_uri provided with request
                return false;
            }

            RedirectUri = redirectUri;

            return Validated();
        }
    }
}
