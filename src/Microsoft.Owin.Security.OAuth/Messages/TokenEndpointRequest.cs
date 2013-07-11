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

            GrantType = getParameter(Constants.Parameters.GrantType);
            ClientId = getParameter(Constants.Parameters.ClientId);
            if (string.Equals(GrantType, Constants.GrantTypes.AuthorizationCode, StringComparison.Ordinal))
            {
                AuthorizationCode = new TokenEndpointRequestAuthorizationCode
                {
                    Code = getParameter(Constants.Parameters.Code),
                    RedirectUri = getParameter(Constants.Parameters.RedirectUri),
                };
            }
            else if (string.Equals(GrantType, Constants.GrantTypes.ClientCredentials, StringComparison.Ordinal))
            {
                ClientCredentials = new TokenEndpointRequestClientCredentials
                {
                    Scope = getParameter(Constants.Parameters.Code)
                };
            }
            else if (string.Equals(GrantType, Constants.GrantTypes.RefreshToken, StringComparison.Ordinal))
            {
                RefreshToken = new TokenEndpointRequestRefreshToken
                {
                    RefreshToken = getParameter(Constants.Parameters.RefreshToken),
                    Scope = getParameter(Constants.Parameters.Scope)
                };
            }
            else if (string.Equals(GrantType, Constants.GrantTypes.Password, StringComparison.Ordinal))
            {
                ResourceOwnerPasswordCredentials = new TokenEndpointRequestResourceOwnerPasswordCredentials
                {
                    UserName = getParameter(Constants.Parameters.Username),
                    Password = getParameter(Constants.Parameters.Password),
                    Scope = getParameter(Constants.Parameters.Scope)
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
