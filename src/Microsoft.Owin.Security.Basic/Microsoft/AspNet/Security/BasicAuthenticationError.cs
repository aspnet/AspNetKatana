// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Net;

namespace Microsoft.AspNet.Security
{
    /// <summary></summary>
    public class BasicAuthenticationError : IBasicAuthenticationError
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _message;

        /// <summary></summary>
        /// <param name="statusCode"></param>
        /// <param name="message"></param>
        public BasicAuthenticationError(HttpStatusCode statusCode, string message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            _statusCode = statusCode;
            _message = message;
        }

        /// <summary></summary>
        public HttpStatusCode StatusCode
        {
            get { return _statusCode; }
        }

        /// <summary></summary>
        public string Message
        {
            get { return _message; }
        }
    }
}
