// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security.Infrastructure;

namespace Microsoft.Owin.Security.OAuth
{
    internal class OAuthBearerAuthenticationHandler : AuthenticationHandler<OAuthBearerAuthenticationOptions>
    {
        private readonly ILogger _logger;
        private readonly string _challenge;

        public OAuthBearerAuthenticationHandler(ILogger logger, string challenge)
        {
            _logger = logger;
            _challenge = challenge;
        }

        protected override async Task<AuthenticationTicket> AuthenticateCore()
        {
            try
            {
                string authorization = Request.Headers.Get("Authorization");
                if (string.IsNullOrEmpty(authorization))
                {
                    return null;
                }

                if (!authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                string protectedText = authorization.Substring("Bearer ".Length).Trim();

                var tokenReceiveContext = new AuthenticationTokenReceiveContext(
                    Context,
                    Options.AccessTokenFormat,
                    protectedText);

                await Options.AccessTokenProvider.ReceiveAsync(tokenReceiveContext);
                if (tokenReceiveContext.Ticket == null)
                {
                    tokenReceiveContext.DeserializeTicket(tokenReceiveContext.Token);
                }

                AuthenticationTicket ticket = tokenReceiveContext.Ticket;
                if (ticket == null)
                {
                    _logger.WriteWarning("invalid bearer token received");
                    return null;
                }

                DateTimeOffset currentUtc = Options.SystemClock.UtcNow;

                if (ticket.Extra.ExpiresUtc.HasValue &&
                    ticket.Extra.ExpiresUtc.Value < currentUtc)
                {
                    _logger.WriteWarning("expired bearer token received");
                    return null;
                }

                var context = new OAuthValidateIdentityContext(ticket.Identity, ticket.Extra.Properties);

                if (Options.Provider != null)
                {
                    await Options.Provider.ValidateIdentity(context);
                }

                return new AuthenticationTicket(context.Identity, context.Extra);
            }
            catch (Exception ex)
            {
                _logger.WriteError(ex.Message);
                // TODO: trace
                return null;
            }
        }

        protected override Task ApplyResponseChallenge()
        {
            if (Response.StatusCode != 401)
            {
                return Task.FromResult<object>(null);
            }

            AuthenticationResponseChallenge challenge = Helper.LookupChallenge(Options.AuthenticationType, Options.AuthenticationMode);

            if (challenge != null)
            {
                Response.Headers.Append("WWW-Authenticate", _challenge);
            }

            return Task.FromResult<object>(null);
        }
    }
}
