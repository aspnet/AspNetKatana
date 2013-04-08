// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Owin.Types;

namespace Microsoft.Owin.Security.Bearer
{
    /// <summary></summary>
    public class BearerAuthenticationMiddleware
    {
        private readonly Func<IDictionary<string, object>, Task> _next;
        private readonly IBearerAuthenticationProtocol _protocol;

        /// <summary></summary>
        /// <param name="next"></param>
        /// <param name="options"></param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Required Katana convention.")]
        public BearerAuthenticationMiddleware(Func<IDictionary<string, object>, Task> next,
            BearerAuthenticationOptions options)
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
                throw new ArgumentException("BearerAuthenticationOptions.Provider must not be null.", "options");
            }

            _next = next;
            _protocol = new BearerAuthenticationProtocol(options.Provider, options.Realm);
        }

        internal BearerAuthenticationMiddleware(Func<IDictionary<string, object>, Task> next,
            IBearerAuthenticationProtocol protocol)
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

            IBearerAuthenticationError error;

            AuthenticationHeaderValue authorization;
            string authorizationHeaderError;

            if (!TryParseAuthorizationHeader(request, out authorization, out authorizationHeaderError))
            {
                error = BearerAuthenticationError.CreateInvalidToken(authorizationHeaderError);
            }
            else
            {
                cancellationToken.ThrowIfCancellationRequested();
                IBearerAuthenticationResult result = await AuthenticateAsync(authorization, cancellationToken);
                IBearerAuthenticationError processingError;

                if (TryProcessResult(result, environment, out processingError))
                {
                    error = null;
                }
                else
                {
                    error = processingError;
                }
            }

            cancellationToken.ThrowIfCancellationRequested();
            AuthenticationHeaderValue challenge = await CreateChallengeAsync(error, cancellationToken);
            string errorMessage;

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

        private async Task<IBearerAuthenticationResult> AuthenticateAsync(AuthenticationHeaderValue authorization,
            CancellationToken cancellationToken)
        {
            IBearerAuthenticationResult result;

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

        private async Task<AuthenticationHeaderValue> CreateChallengeAsync(IBearerAuthenticationError error,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            AuthenticationHeaderValue authenticate = await _protocol.CreateChallengeAsync(cancellationToken);

            if (error == null)
            {
                return authenticate;
            }

            HttpHeaderValueCollection<NameValueHeaderValue> parameter = CreateParameter();
            // TODO: Use TryParseAdd
            parameter.ParseAdd(authenticate.Parameter);

            parameter.Add(new NameValueHeaderValue("error", error.Code));

            if (error.Description != null)
            {
                parameter.Add(new NameValueHeaderValue("error_description", "\"" + error.Description + "\""));
            }

            if (error.Uri != null)
            {
                parameter.Add(new NameValueHeaderValue("error_uri", error.Uri));
            }

            return new AuthenticationHeaderValue(authenticate.Scheme, parameter.ToString());
        }

        private static HttpHeaderValueCollection<NameValueHeaderValue> CreateParameter()
        {
            HttpHeaderValueCollection<NameValueHeaderValue> parameter;

            using (HttpRequestMessage request = new HttpRequestMessage())
            {
                parameter = request.Headers.Pragma;
            }

            return parameter;
        }

        private static bool TryParseAuthorizationHeader(OwinRequest request,
            out AuthenticationHeaderValue authorization, out string error)
        {
            return AuthenticationMiddleware.TryParseAuthorizationHeader(request, out authorization, out error);
        }

        private static bool TryProcessResult(IBearerAuthenticationResult result,
            IDictionary<string, object> environment, out IBearerAuthenticationError error)
        {
            if (result != null)
            {
                error = result.ErrorResult;

                if (error != null)
                {
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

            return true;
        }

        private static bool TryRegisterOnSendingHeaders(AuthenticationHeaderValue challenge,
            OwinRequest request, OwinResponse response, out string errorMessage)
        {
            return AuthenticationMiddleware.TryRegisterAddChallengeOnSendingHeaders(challenge, "Bearer", request,
                response, out errorMessage);
        }

        private static Task WriteMessage(string message, OwinResponse response,
            CancellationToken cancellationToken)
        {
            return AuthenticationMiddleware.WriteBody("text/plain; charset=utf-8", message, new UTF8Encoding(
                encoderShouldEmitUTF8Identifier: false), response, cancellationToken);
        }
    }
}
