// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;

namespace Microsoft.Owin.Security.OAuth
{
    public class OAuthLookupClientContext : BaseValidatingContext<OAuthAuthorizationServerOptions>
    {
        public OAuthLookupClientContext(
            IOwinContext context,
            OAuthAuthorizationServerOptions options,
            ClientDetails requestDetails,
            bool isValidatingRedirectUri,
            bool isValidatingClientSecret)
            : base(context, options)
        {
            RequestDetails = requestDetails;
            IsValidatingRedirectUri = isValidatingRedirectUri;
            IsValidatingClientSecret = isValidatingClientSecret;
        }

        public ClientDetails RequestDetails { get; private set; }

        public ClientDetails FoundDetails { get; private set; }

        public bool IsValidatingRedirectUri { get; private set; }

        public bool IsValidatingClientSecret { get; private set; }

        public string ClientId
        {
            get { return RequestDetails.ClientId; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "This is a string parameter named redirect_uri in the protocol")]
        public string EffectiveRedirectUri { get; private set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "1#", Justification = "This is a string parameter named redirect_uri in the protocol")]
        public void ClientFound(string clientSecret, string redirectUri)
        {
            FoundDetails = new ClientDetails
            {
                ClientId = RequestDetails.ClientId,
                ClientSecret = clientSecret,
                RedirectUri = redirectUri
            };

            if (IsValidatingClientSecret)
            {
                bool acceptable = false;
                if (String.IsNullOrEmpty(RequestDetails.ClientSecret) &&
                    String.IsNullOrEmpty(FoundDetails.ClientSecret))
                {
                    // public client - no credentials provided and none expected
                    acceptable = true;
                }
                else if (!String.IsNullOrEmpty(RequestDetails.ClientSecret) &&
                    !String.IsNullOrEmpty(FoundDetails.ClientSecret) &&
                    String.Equals(RequestDetails.ClientSecret, FoundDetails.ClientSecret, StringComparison.Ordinal))
                {
                    // confidential client - credentials provided and expected and matching
                    acceptable = true;
                }
                if (!acceptable)
                {
                    // all other cases are not validated
                    return;
                }
            }

            if (IsValidatingRedirectUri)
            {
                bool acceptable = false;
                if (String.IsNullOrEmpty(RequestDetails.RedirectUri) &&
                    !String.IsNullOrEmpty(FoundDetails.RedirectUri))
                {
                    // no parameter provided - will use the registered redirect_uri
                    acceptable = true;
                    EffectiveRedirectUri = FoundDetails.RedirectUri;
                }
                else if (!String.IsNullOrEmpty(RequestDetails.RedirectUri) &&
                    String.IsNullOrEmpty(FoundDetails.RedirectUri))
                {
                    // request redirect_uri provided and not registered - taken as stated
                    acceptable = true;
                    EffectiveRedirectUri = RequestDetails.RedirectUri;
                }
                else if (!String.IsNullOrEmpty(RequestDetails.RedirectUri) &&
                    !String.IsNullOrEmpty(FoundDetails.RedirectUri) &&
                    String.Equals(RequestDetails.RedirectUri, FoundDetails.RedirectUri, StringComparison.OrdinalIgnoreCase))
                {
                    acceptable = true;
                    EffectiveRedirectUri = FoundDetails.RedirectUri;
                }
                if (!acceptable)
                {
                    // all other cases are not validated
                    return;
                }
            }

            Validated();
        }
    }
}
