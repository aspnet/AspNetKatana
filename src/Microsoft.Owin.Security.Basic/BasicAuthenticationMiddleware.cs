// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Owin.Security.Basic
{
    /// <summary></summary>
    public class BasicAuthenticationMiddleware
    {
        private readonly Func<IDictionary<string, object>, Task> _next;
        private readonly IBasicAuthenticationProtocol _protocol;

        /// <summary></summary>
        /// <param name="next"></param>
        /// <param name="options"></param>
        public BasicAuthenticationMiddleware(Func<IDictionary<string, object>, Task> next,
            BasicAuthenticationOptions options)
        {
            if (next == null)
            {
                throw new ArgumentNullException("next");
            }

            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            if (options.Provider == null)
            {
                // TODO: Get error messages from resources.
                throw new ArgumentException("BasicAuthenticationOptions.Provider must not be null.", "options");
            }

            _next = next;
            _protocol = new BasicAuthenticationProtocol(options.Provider, options.Realm);
        }

        internal BasicAuthenticationMiddleware(Func<IDictionary<string, object>, Task> next,
            BasicAuthenticationProtocol protocol)
        {
            if (next == null)
            {
                throw new ArgumentNullException("next");
            }

            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }

            _next = next;
            _protocol = protocol;
        }

        /// <summary></summary>
        /// <param name="environment"></param>
        /// <returns></returns>
        public async Task Invoke(IDictionary<string, object> environment)
        {
            OwinRequest request = new OwinRequest(environment);
            OwinResponse response = new OwinResponse(environment);
            CancellationToken cancellationToken = request.CallCancelled;

            AuthenticationHeaderValue authorization;
            string authorizationHeaderError;

            if (!TryParseAuthorizationHeader(request, out authorization, out authorizationHeaderError))
            {
                response.StatusCode = 400;
                cancellationToken.ThrowIfCancellationRequested();
                await WriteMessage(authorizationHeaderError, response, cancellationToken);
                return;
            }

            cancellationToken.ThrowIfCancellationRequested();
            IBasicAuthenticationResult result = await AuthenticateAsync(authorization, cancellationToken);

            int errorStatusCode;
            string errorMessage;

            if (!TryProcessResult(result, environment, out errorStatusCode, out errorMessage))
            {
                response.StatusCode = errorStatusCode;
                cancellationToken.ThrowIfCancellationRequested();
                await AddChallengeOnUnauthorizedAsync(response, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                await WriteMessage(errorMessage, response, cancellationToken);
                return;
            }

            cancellationToken.ThrowIfCancellationRequested();
            AuthenticationHeaderValue challenge = await _protocol.CreateChallengeAsync(cancellationToken);
            if (!TryRegisterOnSendingHeaders(challenge, request, response, out errorMessage))
            {
                response.StatusCode = 500;
                cancellationToken.ThrowIfCancellationRequested();
                await WriteMessage(errorMessage, response, cancellationToken);
                return;
            }

            cancellationToken.ThrowIfCancellationRequested();
            await _next(environment);
        }

        private async Task AddChallengeOnUnauthorizedAsync(OwinResponse response, CancellationToken cancellationToken)
        {
            if (response.StatusCode != 401)
            {
                return;
            }

            cancellationToken.ThrowIfCancellationRequested();
            AuthenticationHeaderValue challenge = await _protocol.CreateChallengeAsync(cancellationToken);
            AuthenticationMiddleware.AddChallenge(challenge, response);
        }

        private async Task<IBasicAuthenticationResult> AuthenticateAsync(AuthenticationHeaderValue authorization,
            CancellationToken cancellationToken)
        {
            IBasicAuthenticationResult result;

            if (authorization != null)
            {
                cancellationToken.ThrowIfCancellationRequested();
                result = await _protocol.AuthenticateAsync(authorization, cancellationToken);
            }
            else
            {
                result = null;
            }

            return result;
        }

        private static bool TryParseAuthorizationHeader(OwinRequest request,
            out AuthenticationHeaderValue authorization, out string error)
        {
            return AuthenticationMiddleware.TryParseAuthorizationHeader(request, out authorization, out error);
        }

        private static bool TryProcessResult(IBasicAuthenticationResult result,
            IDictionary<string, object> environment, out int errorStatusCode, out string errorMessage)
        {
            IBasicAuthenticationError error;

            if (result != null)
            {
                error = result.ErrorResult;

                if (error != null)
                {
                    errorStatusCode = (int)error.StatusCode;
                    errorMessage = error.Message;
                    return false;
                }
                else if (error == null && result.Principal != null)
                {
                    environment["server.User"] = result.Principal;
                }
            }
            else
            {
                error = null;
            }

            errorStatusCode = 0;
            errorMessage = null;
            return true;
        }

        private static bool TryRegisterOnSendingHeaders(AuthenticationHeaderValue challenge, OwinRequest request,
            OwinResponse response, out string errorMessage)
        {
            return AuthenticationMiddleware.TryRegisterAddChallengeOnSendingHeaders(challenge, "Basic", request,
                response, out errorMessage);
        }

        private static Task WriteMessage(string message, OwinResponse response, CancellationToken cancellationToken)
        {
            // Internet Explorer doesn't switch rendering engines well after auth failure -> success.
            const string BodyTemplate = "<html><head><title>{0}</title></head><body>{1}</body></html>";
            string body = string.Format(CultureInfo.InvariantCulture, BodyTemplate, "Authentication Failed", message);
            return AuthenticationMiddleware.WriteBody("text/html; charset=utf-8", body, new UTF8Encoding(
                encoderShouldEmitUTF8Identifier: false), response, cancellationToken);
        }
    }
}
