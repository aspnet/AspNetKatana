// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;

namespace Microsoft.Owin.Security.OAuth.Messages
{
    public class TokenEndpointRequest
    {
        public TokenEndpointRequest(IReadableStringCollection parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }

            Func<string, string> getParameter = parameters.Get;

            GrantType = getParameter("grant_type");
            ClientId = getParameter("client_id");
            if (string.Equals(GrantType, "authorization_code", StringComparison.Ordinal))
            {
                AuthorizationCode = new TokenEndpointRequestAuthorizationCode
                {
                    Code = getParameter("code"),
                    RedirectUri = getParameter("redirect_uri"),
                };
            }
            else if (string.Equals(GrantType, "client_credentials", StringComparison.Ordinal))
            {
                ClientCredentials = new TokenEndpointRequestClientCredentials
                {
                    Scope = getParameter("scope")
                };
            }
            else if (string.Equals(GrantType, "refresh_token", StringComparison.Ordinal))
            {
                RefreshToken = new TokenEndpointRequestRefreshToken
                {
                    RefreshToken = getParameter("refresh_token"),
                    Scope = getParameter("scope")
                };
            }
            else if (string.Equals(GrantType, "password", StringComparison.Ordinal))
            {
                ResourceOwnerPasswordCredentials = new TokenEndpointRequestResourceOwnerPasswordCredentials
                {
                    UserName = getParameter("username"),
                    Password = getParameter("password"),
                    Scope = getParameter("scope")
                };
            }
        }

        public string GrantType { get; private set; }
        public string ClientId { get; private set; }

        public TokenEndpointRequestAuthorizationCode AuthorizationCode { get; private set; }
        public TokenEndpointRequestClientCredentials ClientCredentials { get; private set; }
        public TokenEndpointRequestRefreshToken RefreshToken { get; private set; }
        public TokenEndpointRequestResourceOwnerPasswordCredentials ResourceOwnerPasswordCredentials { get; private set; }

        public bool IsAuthorizationCodeGrantType
        {
            get { return AuthorizationCode != null; }
        }

        public bool IsClientCredentialsGrantType
        {
            get { return ClientCredentials != null; }
        }

        public bool IsRefreshTokenGrantType
        {
            get { return RefreshToken != null; }
        }

        public bool IsResourceOwnerPasswordCredentialsGrantType
        {
            get { return ResourceOwnerPasswordCredentials != null; }
        }
    }
}
