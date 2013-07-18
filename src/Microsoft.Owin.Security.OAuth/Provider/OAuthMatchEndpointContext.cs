// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Owin.Security.Provider;

namespace Microsoft.Owin.Security.OAuth
{
    public class OAuthMatchEndpointContext : BaseContext<OAuthAuthorizationServerOptions>
    {
        public OAuthMatchEndpointContext(
            IOwinContext context,
            OAuthAuthorizationServerOptions options)
            : base(context, options)
        {
        }

        public bool IsAuthorizeEndpoint { get; private set; }

        public bool IsTokenEndpoint { get; private set; }

        public void MatchesAuthorizeEndpoint()
        {
            IsAuthorizeEndpoint = true;
            IsTokenEndpoint = false;
        }

        public void MatchesTokenEndpoint()
        {
            IsAuthorizeEndpoint = false;
            IsTokenEndpoint = true;
        }

        public void MatchesNothing()
        {
            IsAuthorizeEndpoint = false;
            IsTokenEndpoint = false;
        }
    }
}
