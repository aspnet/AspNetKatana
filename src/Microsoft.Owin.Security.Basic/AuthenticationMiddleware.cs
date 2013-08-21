// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Owin.Security
{
    internal static class AuthenticationMiddleware
    {
        public static void AddChallenge(AuthenticationHeaderValue challenge, IOwinResponse response)
        {
            response.Headers.Append("WWW-Authenticate", challenge.ToString());
        }

        public static bool IsAuthenticationMethodEnabled(IOwinRequest request, string authenticationMethod)
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

        public static bool TryParseAuthorizationHeader(IOwinRequest request,
            out AuthenticationHeaderValue authorization, out string error)
        {
            IList<string> unparsedHeaders = request.Headers.GetValues("Authorization");

            string unparsedHeader;

            if (unparsedHeaders != null)
            {
                if (unparsedHeaders.Count == 1)
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
            string authenticationMethod, IOwinContext context, out string errorMessage)
        {
            OnSendingHeadersState state = new OnSendingHeadersState
            {
                Challenge = challenge,
                Context = context
            };

            context.Response.OnSendingHeaders((untypedState) => OnSendingHeaders((OnSendingHeadersState)untypedState,
                authenticationMethod), state);

            errorMessage = null;
            return true;
        }

        public static async Task WriteBody(string contentType, string body, Encoding encoding, IOwinResponse response,
            CancellationToken cancellationToken)
        {
            const int DefaultBufferSize = 0x400;
            response.ContentType = contentType;
            cancellationToken.ThrowIfCancellationRequested();
            using (TextWriter writer = new StreamWriter(response.Body, encoding, DefaultBufferSize, leaveOpen: true))
            {
                await writer.WriteAsync(body);
            }
        }

        private static void OnSendingHeaders(OnSendingHeadersState state, string authenticationMethod)
        {
            AuthenticationHeaderValue challenge = state.Challenge;
            IOwinRequest request = state.Context.Request;
            IOwinResponse response = state.Context.Response;

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
            public IOwinContext Context;
        }
    }
}
