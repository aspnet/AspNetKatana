// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Owin.Security.Provider;

namespace Microsoft.Owin.Security.OAuth
{
    public class OAuthAuthorizeEndpointContext : EndpointContext<OAuthAuthorizationServerOptions>
    {
        public OAuthAuthorizeEndpointContext(IOwinContext context, OAuthAuthorizationServerOptions options)
            : base(context, options)
        {
        }
    }
}
