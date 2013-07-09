// <copyright file="OAuthBearerAuthenticationContext.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

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

            var challenge = Helper.LookupChallenge(Options.AuthenticationType, Options.AuthenticationMode);

            if (challenge != null)
            {
                Response.Headers.Append("WWW-Authenticate", _challenge);
            }

            return Task.FromResult<object>(null);
        }
    }
}
