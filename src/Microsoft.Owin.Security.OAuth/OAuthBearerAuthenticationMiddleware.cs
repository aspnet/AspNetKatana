// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Owin.Logging;
using Microsoft.Owin.Security.DataHandler;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.Infrastructure;
using Owin;

namespace Microsoft.Owin.Security.OAuth
{
    public class OAuthBearerAuthenticationMiddleware : AuthenticationMiddleware<OAuthBearerAuthenticationOptions>
    {
        private readonly ILogger _logger;

        private readonly string _challenge;

        public OAuthBearerAuthenticationMiddleware(
            OwinMiddleware next,
            IAppBuilder app,
            OAuthBearerAuthenticationOptions options)
            : base(next, options)
        {
            _logger = app.CreateLogger<OAuthBearerAuthenticationMiddleware>();

            if (string.IsNullOrWhiteSpace(Options.Realm))
            {
                _challenge = "Bearer";
            }
            else
            {
                _challenge = "Bearer realm=\"" + Options.Realm + "\"";
            }

            if (Options.Provider == null)
            {
                Options.Provider = new OAuthBearerAuthenticationProvider();
            }

            if (Options.AccessTokenFormat == null)
            {
                IDataProtector dataProtecter = app.CreateDataProtector(
                    typeof(OAuthBearerAuthenticationMiddleware).Namespace,
                    "Access Token");
                Options.AccessTokenFormat = new TicketDataFormat(dataProtecter);
            }

            if (Options.AccessTokenProvider == null)
            {
                Options.AccessTokenProvider = new AuthenticationTokenProvider();
            }
        }

        protected override AuthenticationHandler<OAuthBearerAuthenticationOptions> CreateHandler()
        {
            return new OAuthBearerAuthenticationHandler(_logger, _challenge);
        }
    }
}
