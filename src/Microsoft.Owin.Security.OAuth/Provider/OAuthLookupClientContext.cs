// <copyright file="OAuthLookupClientContext.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

#if AUTHSERVER

using System;
using Microsoft.Owin.Security.Provider;

namespace Microsoft.Owin.Security.OAuth
{
    public class OAuthLookupClientContext : BaseContext
    {
        public OAuthLookupClientContext(
            IOwinContext context,
            ClientDetails requestDetails,
            bool isValidatingRedirectUri,
            bool isValidatingClientSecret)
            : base(context)
        {
            RequestDetails = requestDetails;
            IsValidatingRedirectUri = isValidatingRedirectUri;
            IsValidatingClientSecret = isValidatingClientSecret;
        }

        public ClientDetails RequestDetails { get; private set; }

        public ClientDetails FoundDetails { get; private set; }

        public bool IsValidatingRedirectUri { get; private set; }

        public bool IsValidatingClientSecret { get; private set; }

        public bool IsValidated { get; private set; }

        public string ClientId
        {
            get { return RequestDetails.ClientId; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "This is a string parameter named redirect_uri in the protocol")]
        public string EffectiveRedirectUri { get; private set; }

        public void ClientFound(ClientDetails foundDetails)
        {
            FoundDetails = foundDetails;

            if (String.IsNullOrEmpty(RequestDetails.ClientId) ||
                String.IsNullOrEmpty(FoundDetails.ClientId) ||
                !String.Equals(RequestDetails.ClientId, FoundDetails.ClientId, StringComparison.Ordinal))
            {
                // missing or mismatched client id - application lookup error - invalid by default
                return;
            }

            if (IsValidatingClientSecret)
            {
                var acceptable = false;
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
                var acceptable = false;
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

            IsValidated = true;
        }
    }
}

#endif
