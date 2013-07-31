// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.Owin.Security.OAuth
{
    public interface IOAuthAuthorizationServerProvider
    {
        Task MatchEndpoint(OAuthMatchEndpointContext context);
        Task ValidateClientRedirectUri(OAuthValidateClientRedirectUriContext context);
        Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context);

        Task ValidateAuthorizeRequest(OAuthValidateAuthorizeRequestContext context);
        Task ValidateTokenRequest(OAuthValidateTokenRequestContext context);

        Task GrantAuthorizationCode(OAuthGrantAuthorizationCodeContext context);
        Task GrantRefreshToken(OAuthGrantRefreshTokenContext context);
        Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context);
        Task GrantClientCredentials(OAuthGrantClientCredentialsContext context);
        Task GrantCustomExtension(OAuthGrantCustomExtensionContext context);

        Task AuthorizeEndpoint(OAuthAuthorizeEndpointContext context);
        Task TokenEndpoint(OAuthTokenEndpointContext context);
    }
}
