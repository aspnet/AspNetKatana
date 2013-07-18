// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Microsoft.Owin.Security.OAuth
{
    public class OAuthAuthorizationServerProvider : IOAuthAuthorizationServerProvider
    {
        public OAuthAuthorizationServerProvider()
        {
            OnLookupClient = context => Task.FromResult<object>(null);
            OnValidateResourceOwnerCredentials = context => Task.FromResult<object>(null);
            OnValidateClientCredentials = context => Task.FromResult<object>(null);
            OnValidateCustomGrant = context => Task.FromResult<object>(null);
            OnAuthorizeEndpoint = context => Task.FromResult<object>(null);
            OnTokenEndpoint = context => Task.FromResult<object>(null);
        }

        public Func<OAuthLookupClientContext, Task> OnLookupClient { get; set; }
        public Func<OAuthValidateResourceOwnerCredentialsContext, Task> OnValidateResourceOwnerCredentials { get; set; }
        public Func<OAuthValidateClientCredentialsContext, Task> OnValidateClientCredentials { get; set; }
        public Func<OAuthValidateCustomGrantContext, Task> OnValidateCustomGrant { get; set; }
        public Func<OAuthAuthorizeEndpointContext, Task> OnAuthorizeEndpoint { get; set; }
        public Func<OAuthTokenEndpointContext, Task> OnTokenEndpoint { get; set; }

        public virtual Task LookupClient(OAuthLookupClientContext context)
        {
            return OnLookupClient.Invoke(context);
        }

        public virtual Task ValidateResourceOwnerCredentials(OAuthValidateResourceOwnerCredentialsContext context)
        {
            return OnValidateResourceOwnerCredentials.Invoke(context);
        }

        public virtual Task ValidateClientCredentials(OAuthValidateClientCredentialsContext context)
        {
            return OnValidateClientCredentials.Invoke(context);
        }

        public virtual Task ValidateCustomGrant(OAuthValidateCustomGrantContext context)
        {
            return OnValidateCustomGrant.Invoke(context);
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
