// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Net;

namespace Microsoft.Owin.Security.Basic
{
    internal class BasicAuthenticationError : IBasicAuthenticationError
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _message;

        public BasicAuthenticationError(HttpStatusCode statusCode, string message)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            _statusCode = statusCode;
            _message = message;
        }

        public HttpStatusCode StatusCode
        {
            get { return _statusCode; }
        }

        public string Message
        {
            get { return _message; }
        }
    }
}
