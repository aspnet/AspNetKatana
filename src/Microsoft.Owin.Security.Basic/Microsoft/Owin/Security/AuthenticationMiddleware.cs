// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Owin.Types;

namespace Microsoft.Owin.Security
{
    internal static class AuthenticationMiddleware
    {
        public static void AddChallenge(AuthenticationHeaderValue challenge, OwinResponse response)
        {
            response.AddHeader("WWW-Authenticate", challenge.ToString());
        }

        public static bool IsAuthenticationMethodEnabled(OwinRequest request, string authenticationMethod)
        {
            IEnumerable<string> authenticationMethods = request.Get<IEnumerable<string>>(
                AuthenticationKeys.AuthenticationMethods);

            if (authenticationMethods != null && !authenticationMethods.Any(m => String.Equals(authenticationMethod, m,
                StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            return true;
        }

        public static bool TryParseAuthorizationHeader(OwinRequest request,
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

        public static bool TryRegisterAddChallengeOnSendingHeaders(AuthenticationHeaderValue challenge,
            string authenticationMethod, OwinRequest request, OwinResponse response, out string errorMessage)
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

            registerOnSendingHeaders((untypedState) => OnSendingHeaders((OnSendingHeadersState)untypedState,
                authenticationMethod), state);

            errorMessage = null;
            return true;
        }

        public static async Task WriteBody(string contentType, string body, Encoding encoding, OwinResponse response,
            CancellationToken cancellationToken)
        {
            const int DefaultBufferSize = 0x400;
            response.ContentType = contentType;

            using (TextWriter writer = new StreamWriter(response.Body, encoding, DefaultBufferSize, leaveOpen: true))
            {
                await writer.WriteAsync(body);
            }
        }

        private static Action<Action<object>, object> GetRegisterOnSendingHeaders(OwinRequest request)
        {
            return request.Get<Action<Action<object>, object>>(OwinConstants.CommonKeys.OnSendingHeaders);
        }

        private static void OnSendingHeaders(OnSendingHeadersState state, string authenticationMethod)
        {
            AuthenticationHeaderValue challenge = state.Challenge;
            OwinResponse response = state.Response;
            OwinRequest request = state.Request;

            if (response.StatusCode != 401)
            {
                return;
            }

            if (!IsAuthenticationMethodEnabled(request, authenticationMethod))
            {
                return;
            }

            AddChallenge(challenge, response);
        }

        private class OnSendingHeadersState
        {
            public AuthenticationHeaderValue Challenge;
            public OwinResponse Response;
            public OwinRequest Request;
        }
    }
}
