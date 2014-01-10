// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Owin.Security.OAuth
{
    /// <summary>
    /// Provides context information used when handling OAuth extension grant types.
    /// </summary>
    public class OAuthGrantCustomExtensionContext : BaseValidatingTicketContext<OAuthAuthorizationServerOptions>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthGrantCustomExtensionContext"/> class
        /// </summary>
        /// <param name="context"></param>
        /// <param name="options"></param>
        /// <param name="clientId"></param>
        /// <param name="grantType"></param>
        /// <param name="parameters"></param>
        public OAuthGrantCustomExtensionContext(
            IOwinContext context,
            OAuthAuthorizationServerOptions options,
            string clientId,
            string grantType,
            IReadableStringCollection parameters)
            : base(context, options, null)
        {
            ClientId = clientId;
            GrantType = grantType;
            Parameters = parameters;
        }

        /// <summary>
        /// Gets the OAuth client id.
        /// </summary>
        public string ClientId { get; private set; }

        /// <summary>
        /// Gets the name of the OAuth extension grant type.
        /// </summary>
        public string GrantType { get; private set; }

        /// <summary>
        /// Gets a list of additional parameters from the token request.
        /// </summary>
        public IReadableStringCollection Parameters { get; private set; }
    }
}
