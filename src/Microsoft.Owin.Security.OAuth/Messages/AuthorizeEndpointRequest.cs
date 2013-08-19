// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Owin.Security.OAuth.Messages
{
    /// <summary>
    /// Data object representing the information contained in the query string of an Authorize endpoint request.
    /// </summary>
    public class AuthorizeEndpointRequest
    {
        /// <summary>
        /// Creates a new instance populated with values from the query string parameters.
        /// </summary>
        /// <param name="parameters">Query string parameters from a request.</param>
        public AuthorizeEndpointRequest(IReadableStringCollection parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }

            Scope = new List<string>();

            foreach (var parameter in parameters)
            {
                AddParameter(parameter.Key, parameters.Get(parameter.Key));
            }
        }

        /// <summary>
        /// The "response_type" query string parameter of the Authorize request. Known values are "code" and "token".
        /// </summary>
        public string ResponseType { get; set; }

        /// <summary>
        /// The "client_id" query string parameter of the Authorize request. 
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// The "redirect_uri" query string parameter of the Authorize request. May be absent if the server should use the 
        /// redirect uri known to be registered to the client id.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "By design")]
        public string RedirectUri { get; set; }

        /// <summary>
        /// The "scope" query string parameter of the Authorize request. May be absent if the server should use default scopes.
        /// </summary>
        public IList<string> Scope { get; private set; }

        /// <summary>
        /// The "scope" query string parameter of the Authorize request. May be absent if the client does not require state to be 
        /// included when returning to the RedirectUri.
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// True if the "response_type" query string parameter is "code".
        /// See also, http://tools.ietf.org/html/rfc6749#section-4.1.1
        /// </summary>
        public bool IsAuthorizationCodeGrantType
        {
            get { return string.Equals(ResponseType, Constants.ResponseTypes.Code, StringComparison.Ordinal); }
        }

        /// <summary>
        /// True if the "response_type" query string parameter is "token".
        /// See also, http://tools.ietf.org/html/rfc6749#section-4.2.1
        /// </summary>
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
                Scope = value.Split(' ');
            }
            else if (string.Equals(name, Constants.Parameters.State, StringComparison.Ordinal))
            {
                State = value;
            }
        }
    }
}
