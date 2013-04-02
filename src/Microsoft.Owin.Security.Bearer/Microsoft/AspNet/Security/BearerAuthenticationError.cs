// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace Microsoft.AspNet.Security
{
    /// <summary></summary>
    public class BearerAuthenticationError : IBearerAuthenticationError
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _code;
        private readonly string _description;
        private readonly string _uri;

        /// <summary></summary>
        /// <param name="statusCode"></param>
        /// <param name="code"></param>
        public BearerAuthenticationError(HttpStatusCode statusCode, string code)
            : this(statusCode, code, null)
        {
        }

        /// <summary></summary>
        /// <param name="statusCode"></param>
        /// <param name="code"></param>
        /// <param name="description"></param>
        public BearerAuthenticationError(HttpStatusCode statusCode, string code, string description)
            : this(statusCode, code, description, null)
        {
        }

        /// <summary></summary>
        /// <param name="statusCode"></param>
        /// <param name="code"></param>
        /// <param name="description"></param>
        /// <param name="uri"></param>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "3#",
            Justification = "We want to support URIs as strings")]
        public BearerAuthenticationError(HttpStatusCode statusCode, string code, string description, string uri)
        {
            if (code == null)
            {
                throw new ArgumentNullException("code");
            }

            _statusCode = statusCode;
            _code = code;
            _description = description;
            _uri = uri;
        }

        /// <inheritdoc />
        public HttpStatusCode StatusCode
        {
            get { return _statusCode; }
        }

        /// <inheritdoc />
        public string Code
        {
            get { return _code; }
        }

        /// <inheritdoc />
        public string Description
        {
            get { return _description; }
        }

        /// <inheritdoc />
        public string Uri
        {
            get { return _uri; }
        }

        /// <summary></summary>
        /// <returns></returns>
        public static BearerAuthenticationError CreateInvalidRequest()
        {
            return CreateInvalidRequest(null);
        }

        /// <summary></summary>
        /// <param name="errorDescription"></param>
        /// <returns></returns>
        public static BearerAuthenticationError CreateInvalidRequest(string errorDescription)
        {
            return CreateInvalidRequest(errorDescription, null);
        }

        /// <summary></summary>
        /// <param name="errorDescription"></param>
        /// <param name="errorUri"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "1#",
            Justification = "We want to support URIs as strings")]
        public static BearerAuthenticationError CreateInvalidRequest(string errorDescription, string errorUri)
        {
            return new BearerAuthenticationError(HttpStatusCode.BadRequest, "invalid_request", errorDescription,
                errorUri);
        }

        /// <summary></summary>
        /// <returns></returns>
        public static BearerAuthenticationError CreateInvalidToken()
        {
            return CreateInvalidToken(null);
        }

        /// <summary></summary>
        /// <param name="errorDescription"></param>
        /// <returns></returns>
        public static BearerAuthenticationError CreateInvalidToken(string errorDescription)
        {
            return CreateInvalidToken(errorDescription, null);
        }

        /// <summary></summary>
        /// <param name="errorDescription"></param>
        /// <param name="errorUri"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "1#",
            Justification = "We want to support URIs as strings")]
        public static BearerAuthenticationError CreateInvalidToken(string errorDescription, string errorUri)
        {
            return new BearerAuthenticationError(HttpStatusCode.Unauthorized, "invalid_token", errorDescription,
                errorUri);
        }
    }
}
