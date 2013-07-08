// <copyright file="OAuthValidateClientCredentialsContext.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Diagnostics.CodeAnalysis;
using Microsoft.Owin.Security.Provider;

namespace Microsoft.Owin.Security.OAuth
{
    public class OAuthValidateClientCredentialsContext : BaseContext
    {
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "3#", Justification = "By design")]
        public OAuthValidateClientCredentialsContext(
            IOwinContext context, 
            string clientId, 
            string clientSecret, 
            string redirectUri)
            : base(context)
        {
            ClientId = clientId;
            ClientSecret = clientSecret;
            RedirectUri = redirectUri;
        }

        public string ClientId { get; private set; }
        public string ClientSecret { get; private set; }

        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "By design")]
        public string RedirectUri { get; private set; }

        public bool IsValidated { get; private set; }

        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "1#", Justification = "By design")]
        public void ClientFound(string clientSecret, string redirectUri)
        {
            if (ClientSecret != null && !String.Equals(ClientSecret, clientSecret, StringComparison.Ordinal))
            {
                return;
            }
            if (RedirectUri != null && !String.Equals(RedirectUri, redirectUri, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            ClientSecret = clientSecret;
            RedirectUri = redirectUri;
            IsValidated = true;
        }
    }
}

#endif
