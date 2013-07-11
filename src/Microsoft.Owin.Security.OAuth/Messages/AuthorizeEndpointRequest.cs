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
            get { return string.Equals(ResponseType, Constants.ResponseTypes.Code, StringComparison.Ordinal); }
        }

        public bool IsImplicitGrantType
        {
            get { return string.Equals(ResponseType, Constants.ResponseTypes.Token, StringComparison.Ordinal); }
        }

        private void AddParameter(string name, string value)
        {
            if (string.Equals(name, Constants.Parameters.ResponseType, StringComparison.Ordinal))
            {
                ResponseType = value;
            }
            else if (string.Equals(name, Constants.Parameters.ClientId, StringComparison.Ordinal))
            {
                ClientId = value;
            }
            else if (string.Equals(name, Constants.Parameters.RedirectUri, StringComparison.Ordinal))
            {
                RedirectUri = value;
            }
            else if (string.Equals(name, Constants.Parameters.Scope, StringComparison.Ordinal))
            {
                Scope = value;
            }
            else if (string.Equals(name, Constants.Parameters.State, StringComparison.Ordinal))
            {
                State = value;
            }
        }
    }
}
