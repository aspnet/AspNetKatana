// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Security
{
    /// <summary></summary>
    public sealed class BasicAuthenticationProtocol : IBasicAuthenticationProtocol
    {
        private readonly IBasicAuthenticationProvider _provider;
        private readonly string _realm;

        /// <summary></summary>
        /// <param name="provider"></param>
        public BasicAuthenticationProtocol(IBasicAuthenticationProvider provider)
            : this(provider, null)
        {
        }

        /// <summary></summary>
        /// <param name="provider"></param>
        /// <param name="realm"></param>
        public BasicAuthenticationProtocol(IBasicAuthenticationProvider provider, string realm)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }

            _provider = provider;
            _realm = realm;
        }

        /// <inheritdoc />
        public async Task<IBasicAuthenticationResult> AuthenticateAsync(AuthenticationHeaderValue authorization,
            CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                return null;
            }

            // TODO: Check before dotting-through potential nulls.
            if (!String.Equals("Basic", authorization.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            string parameter = authorization.Parameter;

            if (parameter == null)
            {
                // TODO: Get error messages from resources.
                return CreateFailedResult(HttpStatusCode.BadRequest, "Invalid parameter");
            }

            byte[] parameterBytes;

            try
            {
                parameterBytes = Convert.FromBase64String(parameter);
            }
            catch (FormatException)
            {
                return CreateFailedResult(HttpStatusCode.BadRequest, "Error decoding base64 string");
            }

            // TODO: Per RFC 2616, "Words of *TEXT MAY contain characters from character sets other than ISO-8859-1
            // only when encoded according to the rules of RFC 2047." Determine whether/how to support other encodings.
            Encoding encoding = (Encoding)Encoding.GetEncoding("iso-8859-1").Clone();
            encoding.DecoderFallback = new DecoderExceptionFallback();
            string decoded;

            try
            {
                decoded = encoding.GetString(parameterBytes);
            }
            catch (DecoderFallbackException)
            {
                return CreateFailedResult(HttpStatusCode.BadRequest, "Error decoding text as ASCII");
            }

            int colonIndex = decoded.IndexOf(':');

            if (colonIndex == -1)
            {
                return CreateFailedResult(HttpStatusCode.BadRequest, "Invalid Basic auth parameter value");
            }

            string username = decoded.Substring(0, colonIndex);
            string password = decoded.Substring(colonIndex + 1);

            IPrincipal principal = await _provider.AuthenticateAsync(username, password, cancellationToken);

            if (principal == null)
            {
                return CreateFailedResult(HttpStatusCode.Unauthorized, "Invalid username or password");
            }

            return CreateSucceededResult(principal);
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
                return new AuthenticationHeaderValue("Basic");
            }
            else
            {
                return new AuthenticationHeaderValue("Basic", "realm=\"" + _realm + "\"");
            }
        }

        private static IBasicAuthenticationResult CreateFailedResult(HttpStatusCode statusCode, string message)
        {
            return new BasicAuthenticationResult(new BasicAuthenticationError(statusCode, message));
        }

        private static IBasicAuthenticationResult CreateSucceededResult(IPrincipal principal)
        {
            return new BasicAuthenticationResult(principal);
        }
    }
}
