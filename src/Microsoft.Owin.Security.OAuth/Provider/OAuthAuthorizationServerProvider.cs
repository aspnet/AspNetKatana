// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Microsoft.Owin.Security.OAuth
{
    public class OAuthAuthorizationServerProvider : IOAuthAuthorizationServerProvider
    {
        public OAuthAuthorizationServerProvider()
        {
            OnMatchEndpoint = context => Task.FromResult<object>(null);
            OnLookupClient = context => Task.FromResult<object>(null);

            OnValidateAuthorizeRequest = context => Task.FromResult<object>(null);
            OnValidateTokenRequest = context => Task.FromResult<object>(null);

            OnGrantAuthorizationCode = context => Task.FromResult<object>(null);
            OnGrantResourceOwnerCredentials = context => Task.FromResult<object>(null);
            OnGrantRefreshToken = context => Task.FromResult<object>(null);
            OnGrantClientCredentials = context => Task.FromResult<object>(null);
            OnGrantCustomExtension = context => Task.FromResult<object>(null);

            OnAuthorizeEndpoint = context => Task.FromResult<object>(null);
            OnTokenEndpoint = context => Task.FromResult<object>(null);
        }

        public Func<OAuthMatchEndpointContext, Task> OnMatchEndpoint { get; set; }
        public Func<OAuthLookupClientContext, Task> OnLookupClient { get; set; }

        public Func<OAuthValidateAuthorizeRequestContext, Task> OnValidateAuthorizeRequest { get; set; }
        public Func<OAuthValidateTokenRequestContext, Task> OnValidateTokenRequest { get; set; }

        public Func<OAuthGrantAuthorizationCodeContext, Task> OnGrantAuthorizationCode { get; set; }
        public Func<OAuthGrantResourceOwnerCredentialsContext, Task> OnGrantResourceOwnerCredentials { get; set; }
        public Func<OAuthGrantClientCredentialsContext, Task> OnGrantClientCredentials { get; set; }
        public Func<OAuthGrantRefreshTokenContext, Task> OnGrantRefreshToken { get; set; }
        public Func<OAuthGrantCustomExtensionContext, Task> OnGrantCustomExtension { get; set; }

        public Func<OAuthAuthorizeEndpointContext, Task> OnAuthorizeEndpoint { get; set; }
        public Func<OAuthTokenEndpointContext, Task> OnTokenEndpoint { get; set; }

        public virtual Task MatchEndpoint(OAuthMatchEndpointContext context)
        {
            return OnMatchEndpoint.Invoke(context);
        }

        public virtual Task LookupClient(OAuthLookupClientContext context)
        {
            return OnLookupClient.Invoke(context);
        }

        public Task ValidateAuthorizeRequest(OAuthValidateAuthorizeRequestContext context)
        {
            return OnValidateAuthorizeRequest.Invoke(context);
        }

        public Task ValidateTokenRequest(OAuthValidateTokenRequestContext context)
        {
            return OnValidateTokenRequest.Invoke(context);
        }

        public virtual Task GrantAuthorizationCode(OAuthGrantAuthorizationCodeContext context)
        {
            return OnGrantAuthorizationCode.Invoke(context);
        }

        public virtual Task GrantRefreshToken(OAuthGrantRefreshTokenContext context)
        {
            return OnGrantRefreshToken.Invoke(context);
        }

        public virtual Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            return OnGrantResourceOwnerCredentials.Invoke(context);
        }

        public virtual Task GrantClientCredentials(OAuthGrantClientCredentialsContext context)
        {
            return OnGrantClientCredentials.Invoke(context);
        }

        public virtual Task GrantCustomExtension(OAuthGrantCustomExtensionContext context)
        {
            return OnGrantCustomExtension.Invoke(context);
        }

        public virtual Task AuthorizeEndpoint(OAuthAuthorizeEndpointContext context)
        {
            return OnAuthorizeEndpoint.Invoke(context);
        }

        public virtual Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            return OnTokenEndpoint.Invoke(context);
        }
    }
}
