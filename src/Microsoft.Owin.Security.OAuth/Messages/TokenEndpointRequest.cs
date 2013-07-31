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

            Parameters = parameters;
            GrantType = getParameter(Constants.Parameters.GrantType);
            ClientId = getParameter(Constants.Parameters.ClientId);
            if (String.Equals(GrantType, Constants.GrantTypes.AuthorizationCode, StringComparison.Ordinal))
            {
                AuthorizationCodeGrant = new TokenEndpointRequestAuthorizationCode
                {
                    Code = getParameter(Constants.Parameters.Code),
                    RedirectUri = getParameter(Constants.Parameters.RedirectUri),
                };
            }
            else if (String.Equals(GrantType, Constants.GrantTypes.ClientCredentials, StringComparison.Ordinal))
            {
                ClientCredentialsGrant = new TokenEndpointRequestClientCredentials
                {
                    Scope = (getParameter(Constants.Parameters.Code) ?? string.Empty).Split(' ')
                };
            }
            else if (String.Equals(GrantType, Constants.GrantTypes.RefreshToken, StringComparison.Ordinal))
            {
                RefreshTokenGrant = new TokenEndpointRequestRefreshToken
                {
                    RefreshToken = getParameter(Constants.Parameters.RefreshToken),
                    Scope = (getParameter(Constants.Parameters.Scope) ?? string.Empty).Split(' ')
                };
            }
            else if (String.Equals(GrantType, Constants.GrantTypes.Password, StringComparison.Ordinal))
            {
                ResourceOwnerPasswordCredentialsGrant = new TokenEndpointRequestResourceOwnerPasswordCredentials
                {
                    UserName = getParameter(Constants.Parameters.Username),
                    Password = getParameter(Constants.Parameters.Password),
                    Scope = (getParameter(Constants.Parameters.Scope) ?? string.Empty).Split(' ')
                };
            }
            else if (!String.IsNullOrEmpty(GrantType))
            {
                CustomExtensionGrant = new TokenEndpointRequestCustomExtension
                {
                    Parameters = parameters,
                };
            }
        }

        public IReadableStringCollection Parameters { get; private set; }
        public string GrantType { get; private set; }
        public string ClientId { get; private set; }

        public TokenEndpointRequestAuthorizationCode AuthorizationCodeGrant { get; private set; }
        public TokenEndpointRequestClientCredentials ClientCredentialsGrant { get; private set; }
        public TokenEndpointRequestRefreshToken RefreshTokenGrant { get; private set; }
        public TokenEndpointRequestResourceOwnerPasswordCredentials ResourceOwnerPasswordCredentialsGrant { get; private set; }
        public TokenEndpointRequestCustomExtension CustomExtensionGrant { get; private set; }

        public bool IsAuthorizationCodeGrantType
        {
            get { return AuthorizationCodeGrant != null; }
        }

        public bool IsClientCredentialsGrantType
        {
            get { return ClientCredentialsGrant != null; }
        }

        public bool IsRefreshTokenGrantType
        {
            get { return RefreshTokenGrant != null; }
        }

        public bool IsResourceOwnerPasswordCredentialsGrantType
        {
            get { return ResourceOwnerPasswordCredentialsGrant != null; }
        }

        public bool IsCustomExtensionGrantType
        {
            get { return CustomExtensionGrant != null; }
        }
    }
}
