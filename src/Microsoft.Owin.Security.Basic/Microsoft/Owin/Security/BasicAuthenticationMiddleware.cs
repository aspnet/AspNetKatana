// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.Security;
using Owin.Types;

namespace Microsoft.Owin.Security
{
    /// <summary></summary>
    public class BasicAuthenticationMiddleware
    {
        private readonly Func<IDictionary<string, object>, Task> _next;
        private readonly IBasicAuthenticationProtocol _protocol;

        /// <summary></summary>
        /// <param name="next"></param>
        /// <param name="options"></param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Required Katana convention.")]
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

            if (options.Protocol == null)
            {
                // TODO: Get error messages from resources.
                throw new ArgumentException("BasicAuthenticationOptions.Protocol must not be null.", "options");
            }

            _next = next;
            _protocol = options.Protocol;
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
            AddChallenge(challenge, response);
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

        private static void AddChallenge(AuthenticationHeaderValue challenge, OwinResponse response)
        {
            response.AddHeader("WWW-Authenticate", challenge.ToString());
        }

        private static Action<Action<object>, object> GetRegisterOnSendingHeaders(OwinRequest request)
        {
            return request.Get<Action<Action<object>, object>>(OwinConstants.CommonKeys.OnSendingHeaders);
        }

        private static void OnSendingHeaders(object state)
        {
            OnSendingHeadersState typedState = (OnSendingHeadersState)state;
            AuthenticationHeaderValue challenge = typedState.Challenge;
            OwinResponse response = typedState.Response;
            OwinRequest request = typedState.Request;

            if (response.StatusCode != 401)
            {
                return;
            }

            IEnumerable<string> authenticationMethods = request.Get<IEnumerable<string>>(
                AuthenticationKeys.AuthenticationMethods);

            if (authenticationMethods != null && !authenticationMethods.Any(m => String.Equals("Basic", m,
                StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            AddChallenge(challenge, response);
        }

        private static bool TryParseAuthorizationHeader(OwinRequest request,
            out AuthenticationHeaderValue authorization, out string error)
        {
            string[] unparsedHeaders = request.GetHeaderUnmodified("Authorization");

            string unparsedHeader;

            if (unparsedHeaders != null)
            {
                if (unparsedHeaders.Length == 1)
                {
                    unparsedHeader = unparsedHeaders[0];
                }
                else
                {
                    authorization = null;
                    // TODO: Get error messages from resources.
                    error = "Only one Authorization header may be present.";
                    return false;
                }
            }
            else
            {
                unparsedHeader = null;
            }

            AuthenticationHeaderValue parsedHeader;

            if (unparsedHeader != null)
            {
                if (!AuthenticationHeaderValue.TryParse(unparsedHeader, out parsedHeader))
                {
                    authorization = null;
                    // TODO: Get error messages from resources.
                    error = "The Authorization header is malformed.";
                    return false;
                }
            }
            else
            {
                parsedHeader = null;
            }

            authorization = parsedHeader;
            error = null;
            return true;
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
            Action<Action<object>, object> registerOnSendingHeaders = GetRegisterOnSendingHeaders(request);

            if (registerOnSendingHeaders == null)
            {
                errorMessage = "OnSendingHeaders is not available.";
                return false;
            }

            OnSendingHeadersState state = new OnSendingHeadersState
            {
                Challenge = challenge,
                Response = response,
                Request = request
            };

            registerOnSendingHeaders(OnSendingHeaders, state);

            errorMessage = null;
            return true;
        }

        private static async Task WriteMessage(string message, OwinResponse response,
            CancellationToken cancellationToken)
        {
            const int DefaultBufferSize = 0x400;
            response.ContentType = "text/html; charset=utf-8";

            using (TextWriter writer = new StreamWriter(response.Body, new UTF8Encoding(
                encoderShouldEmitUTF8Identifier: false), DefaultBufferSize, leaveOpen: true))
            {
                // Internet Explorer doesn't switch rendering engines well after auth failure -> success.
                const string MessageTemplate = "<html><head><title>{0}</title></head><body>{1}</body></html>";
                string formattedMessage = string.Format(CultureInfo.InvariantCulture, MessageTemplate,
                    "Authentication Failed", message);
                await writer.WriteAsync(formattedMessage);
            }
        }

        private class OnSendingHeadersState
        {
            public AuthenticationHeaderValue Challenge;
            public OwinResponse Response;
            public OwinRequest Request;
        }
    }
}
