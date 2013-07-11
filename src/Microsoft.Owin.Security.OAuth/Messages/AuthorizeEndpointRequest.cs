// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Owin.Security.OAuth.Messages
{
    public class AuthorizeEndpointRequest
    {
        public AuthorizeEndpointRequest(IReadableStringCollection parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }

            foreach (var parameter in parameters)
            {
                AddParameter(parameter.Key, parameters.Get(parameter.Key));
            }
        }

        public string ResponseType { get; set; }
        public string ClientId { get; set; }

        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "By design")]
        public string RedirectUri { get; set; }

        public string Scope { get; set; }
        public string State { get; set; }

        public bool IsAuthorizationCodeGrantType
        {
            get { return string.Equals(ResponseType, "code", StringComparison.Ordinal); }
        }

        public bool IsImplicitGrantType
        {
            get { return string.Equals(ResponseType, "token", StringComparison.Ordinal); }
        }

        private void AddParameter(string name, string value)
        {
            if (string.Equals(name, "response_type", StringComparison.Ordinal))
            {
                ResponseType = value;
            }
            else if (string.Equals(name, "client_id", StringComparison.Ordinal))
            {
                ClientId = value;
            }
            else if (string.Equals(name, "redirect_uri", StringComparison.Ordinal))
            {
                RedirectUri = value;
            }
            else if (string.Equals(name, "scope", StringComparison.Ordinal))
            {
                Scope = value;
            }
            else if (string.Equals(name, "state", StringComparison.Ordinal))
            {
                State = value;
            }
        }
    }
}
