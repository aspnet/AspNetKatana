// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.Owin.Security.OAuth
{
    public interface IOAuthAuthorizationServerProvider
    {
        Task LookupClient(OAuthLookupClientContext context);
        Task ValidateResourceOwnerCredentials(OAuthValidateResourceOwnerCredentialsContext context);
        Task ValidateClientCredentials(OAuthValidateClientCredentialsContext context);
        Task ValidateCustomGrant(OAuthValidateCustomGrantContext context);
        Task AuthorizeEndpoint(OAuthAuthorizeEndpointContext context);
        Task TokenEndpoint(OAuthTokenEndpointContext context);
    }
}
