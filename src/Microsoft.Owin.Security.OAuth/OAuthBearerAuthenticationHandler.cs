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

namespace Microsoft.Owin.Security.OAuth
{
    internal class OAuthBearerAuthenticationHandler : AuthenticationHandler<OAuthBearerAuthenticationOptions>
    {
        private readonly string _challenge;

        public OAuthBearerAuthenticationHandler(string challenge)
        {
            _challenge = challenge;
        }

        protected override async Task<AuthenticationTicket> AuthenticateCore()
        {
            try
            {
                string authorization = Request.GetHeader("Authorization");

                if (authorization == null || !authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                string protectedText = authorization.Substring("Bearer ".Length).Trim();
                AuthenticationTicket ticket = Options.AccessTokenHandler.Unprotect(protectedText);

                var context = new OAuthValidateIdentityContext(ticket.Identity, ticket.Extra.Properties);

                if (Options.Provider != null)
                {
                    await Options.Provider.ValidateIdentity(context);
                }

                return new AuthenticationTicket(context.Identity, context.Extra);
            }
            catch (Exception ex)
            {
                // TODO: trace
                return null;
            }
        }

        protected override async Task ApplyResponseChallenge()
        {
            if (Response.StatusCode != 401)
            {
                return;
            }

            var challenge = Helper.LookupChallenge(Options.AuthenticationType, Options.AuthenticationMode);

            if (challenge != null)
            {
                Response.AddHeader("WWW-Authenticate", _challenge);
            }
        }
    }
}
