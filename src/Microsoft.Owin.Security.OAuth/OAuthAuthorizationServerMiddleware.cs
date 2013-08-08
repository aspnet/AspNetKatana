// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Owin.Logging;
using Microsoft.Owin.Security.DataHandler;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.Infrastructure;
using Owin;

namespace Microsoft.Owin.Security.OAuth
{
    /// <summary>
    /// Authorization Server middleware component which is added to an OWIN pipeline. This class is not
    /// created by application code directly, instead it is added by calling the the IAppBuilder UseOAuthAuthorizationServer 
    /// extension method.
    /// </summary>
    public class OAuthAuthorizationServerMiddleware : AuthenticationMiddleware<OAuthAuthorizationServerOptions>
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Authorization Server middleware component which is added to an OWIN pipeline. This constructor is not
        /// called by application code directly, instead it is added by calling the the IAppBuilder UseOAuthAuthorizationServer 
        /// extension method.
        /// </summary>
        public OAuthAuthorizationServerMiddleware(
            OwinMiddleware next,
            IAppBuilder app,
            OAuthAuthorizationServerOptions options)
            : base(next, options)
        {
            _logger = app.CreateLogger<OAuthAuthorizationServerMiddleware>();

            if (Options.Provider == null)
            {
                Options.Provider = new OAuthAuthorizationServerProvider();
            }
            if (Options.AuthorizationCodeFormat == null)
            {
                IDataProtector dataProtecter = app.CreateDataProtector(
                    typeof(OAuthAuthorizationServerMiddleware).FullName,
                    "Authentication_Code", "v1");

                Options.AuthorizationCodeFormat = new TicketDataFormat(dataProtecter);
            }
            if (Options.AccessTokenFormat == null)
            {
                IDataProtector dataProtecter = app.CreateDataProtector(
                    typeof(OAuthAuthorizationServerMiddleware).Namespace,
                    "Access_Token", "v1");
                Options.AccessTokenFormat = new TicketDataFormat(dataProtecter);
            }
            if (Options.RefreshTokenFormat == null)
            {
                IDataProtector dataProtecter = app.CreateDataProtector(
                    typeof(OAuthAuthorizationServerMiddleware).Namespace,
                    "Refresh_Token", "v1");
                Options.RefreshTokenFormat = new TicketDataFormat(dataProtecter);
            }
            if (Options.AuthorizationCodeProvider == null)
            {
                Options.AuthorizationCodeProvider = new AuthenticationTokenProvider();
            }
            if (Options.AccessTokenProvider == null)
            {
                Options.AccessTokenProvider = new AuthenticationTokenProvider();
            }
            if (Options.RefreshTokenProvider == null)
            {
                Options.RefreshTokenProvider = new AuthenticationTokenProvider();
            }
        }

        /// <summary>
        /// Called by the AuthenticationMiddleware base class to create a per-request handler. 
        /// </summary>
        /// <returns>A new instance of the request handler</returns>
        protected override AuthenticationHandler<OAuthAuthorizationServerOptions> CreateHandler()
        {
            return new OAuthAuthorizationServerHandler(_logger);
        }
    }
}
