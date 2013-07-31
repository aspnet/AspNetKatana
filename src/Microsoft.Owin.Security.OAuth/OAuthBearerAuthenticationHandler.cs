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

        protected override async Task<AuthenticationTicket> AuthenticateCoreAsync()
        {
            try
            {
                // Find token in default location
                string requestToken = null;
                string authorization = Request.Headers.Get("Authorization");
                if (!string.IsNullOrEmpty(authorization))
                {
                    if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    {
                        requestToken = authorization.Substring("Bearer ".Length).Trim();
                    }
                }

                // Give application opportinity to find from a different location, adjust, or reject token
                var requestTokenContext = new OAuthRequestTokenContext(Context, requestToken);
                await Options.Provider.RequestToken(requestTokenContext);

                // If no token found, no further work possible
                if (string.IsNullOrEmpty(requestTokenContext.Token))
                {
                    return null;
                }

                // Call provider to process the token into data
                var tokenReceiveContext = new AuthenticationTokenReceiveContext(
                    Context,
                    Options.AccessTokenFormat,
                    requestTokenContext.Token);

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

                // Validate expiration time if present
                DateTimeOffset currentUtc = Options.SystemClock.UtcNow;

                if (ticket.Properties.ExpiresUtc.HasValue &&
                    ticket.Properties.ExpiresUtc.Value < currentUtc)
                {
                    _logger.WriteWarning("expired bearer token received");
                    return null;
                }

                // Give application final opportinity to override results
                var context = new OAuthValidateIdentityContext(Context, Options, ticket);
                if (ticket != null &&
                    ticket.Identity != null &&
                    ticket.Identity.IsAuthenticated)
                {
                    // bearer token with identity starts validated
                    context.Validated();
                }
                if (Options.Provider != null)
                {
                    await Options.Provider.ValidateIdentity(context);
                }
                if (!context.IsValidated)
                {
                    return null;
                }

                // resulting identity values go back to caller
                return context.Ticket;
            }
            catch (Exception ex)
            {
                _logger.WriteError(ex.Message);
                // TODO: trace
                return null;
            }
        }

        protected override Task ApplyResponseChallengeAsync()
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
