// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.ObjectModel;
using System.IdentityModel.Tokens;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Security
{
    /// <summary></summary>
    public class IdentityModelBearerAuthenticationProvider : IBearerAuthenticationProvider
    {
        private readonly SecurityTokenHandlerCollection _handlers;

        /// <summary></summary>
        /// <param name="handlers"></param>
        public IdentityModelBearerAuthenticationProvider(SecurityTokenHandlerCollection handlers)
        {
            if (handlers == null)
            {
                throw new ArgumentNullException("handlers");
            }

            _handlers = handlers;
        }

        /// <summary></summary>
        public SecurityTokenHandlerCollection Handlers
        {
            get
            {
                return _handlers;
            }
        }

        /// <inheritdoc />
        public Task<IBearerAuthenticationResult> AuthenticateAsync(string token,
            CancellationToken cancellationToken)
        {
            if (!_handlers.CanReadToken(token))
            {
                return InvalidRequest("The token format is not recognized.");
            }

            SecurityToken parsedToken;

            try
            {
                parsedToken = _handlers.ReadToken(token);
            }
            catch (ArgumentException ex)
            {
                return InvalidRequest(ex.Message);
            }
            catch (SecurityTokenException ex)
            {
                return InvalidRequest(ex.Message);
            }

            ReadOnlyCollection<ClaimsIdentity> identities;

            try
            {
                identities = _handlers.ValidateToken(parsedToken);
            }
            catch (ArgumentException ex)
            {
                return InvalidToken(ex.Message);
            }
            catch (SecurityTokenValidationException ex)
            {
                return InvalidToken(ex.Message);
            }

            return Succeeded(new ClaimsPrincipal(identities));
        }

        private static Task<IBearerAuthenticationResult> InvalidRequest(string description)
        {
            IBearerAuthenticationError error = BearerAuthenticationError.CreateInvalidToken(description);
            IBearerAuthenticationResult result = new BearerAuthenticationResult(error);
            return Task.FromResult(result);
        }

        private static Task<IBearerAuthenticationResult> InvalidToken(string description)
        {
            IBearerAuthenticationError error = BearerAuthenticationError.CreateInvalidToken(description);
            IBearerAuthenticationResult result = new BearerAuthenticationResult(error);
            return Task.FromResult(result);
        }

        private static Task<IBearerAuthenticationResult> Succeeded(IPrincipal principal)
        {
            IBearerAuthenticationResult result = new BearerAuthenticationResult(principal);
            return Task.FromResult(result);
        }
    }
}
