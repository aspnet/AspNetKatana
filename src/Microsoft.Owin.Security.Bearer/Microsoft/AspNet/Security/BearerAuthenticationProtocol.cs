// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Security
{
    /// <summary></summary>
    public sealed class BearerAuthenticationProtocol : IBearerAuthenticationProtocol
    {
        private readonly IBearerAuthenticationProvider _provider;
        private readonly string _realm;

        /// <summary></summary>
        /// <param name="provider"></param>
        public BearerAuthenticationProtocol(IBearerAuthenticationProvider provider)
            : this(provider, null)
        {
        }

        /// <summary></summary>
        /// <param name="provider"></param>
        /// <param name="realm"></param>
        public BearerAuthenticationProtocol(IBearerAuthenticationProvider provider, string realm)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }

            _provider = provider;
            _realm = realm;
        }

        /// <inheritdoc />
        public Task<IBearerAuthenticationResult> AuthenticateAsync(AuthenticationHeaderValue authorization,
            CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException("authorization");
            }

            if (!String.Equals("Bearer", authorization.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult<IBearerAuthenticationResult>(null);
            }

            string token = authorization.Parameter;

            if (token == null)
            {
                // TODO: Get error messages from resources; use language from acceptLanguage header.
                return CreateInvalidRequestResult("Invalid parameter");
            }

            return _provider.AuthenticateAsync(token, cancellationToken);
        }

        /// <inheritdoc />
        public Task<AuthenticationHeaderValue> CreateChallengeAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(CreateChallenge());
        }

        private AuthenticationHeaderValue CreateChallenge()
        {
            if (_realm == null)
            {
                return new AuthenticationHeaderValue("Bearer");
            }
            else
            {
                return new AuthenticationHeaderValue("Bearer", "realm=\"" + _realm + "\"");
            }
        }

        private static Task<IBearerAuthenticationResult> CreateInvalidRequestResult(string errorDescription)
        {
            IBearerAuthenticationError error = BearerAuthenticationError.CreateInvalidRequest(errorDescription);
            IBearerAuthenticationResult result = new BearerAuthenticationResult(error);
            return Task.FromResult(result);
        }
    }
}
