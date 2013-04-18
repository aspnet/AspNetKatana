// <copyright file="AccessTokenRequest.cs" company="Microsoft Open Technologies, Inc.">
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

using System;

namespace Microsoft.Owin.Security.OAuth.Messages
{
    public abstract class AccessTokenRequest
    {
        public string GrantType { get; set; }

        public static AccessTokenRequest Create(Func<string, string> getParameter)
        {
            string grantType = getParameter("grant_type");
            if (string.Equals(grantType, "authorization_code", StringComparison.Ordinal))
            {
                return new AuthorizationCodeAccessTokenRequest
                {
                    GrantType = grantType,
                    Code = getParameter("code"),
                    RedirectUri = getParameter("redirect_uri"),
                    ClientId = getParameter("client_id")
                };
            }
            if (string.Equals(grantType, "password", StringComparison.Ordinal))
            {
                return new ResourceOwnerPasswordCredentialsAccessTokenRequest
                {
                    GrantType = grantType,
                    Username = getParameter("username"),
                    Password = getParameter("password"),
                    Scope = getParameter("scope")
                };
            }
            if (string.Equals(grantType, "client_credentials", StringComparison.Ordinal))
            {
                return new ClientCredentialsAccessTokenRequest
                {
                    GrantType = grantType,
                    Scope = getParameter("scope")
                };
            }
            throw new NotImplementedException("oauth error");
        }
    }
}
