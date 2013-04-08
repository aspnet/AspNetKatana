// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Security.Principal;

namespace Microsoft.Owin.Security.Basic
{
    internal class BasicAuthenticationResult : IBasicAuthenticationResult
    {
        private readonly IBasicAuthenticationError _error;
        private readonly IPrincipal _principal;

        public BasicAuthenticationResult(IPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException("principal");
            }

            _principal = principal;
        }

        public BasicAuthenticationResult(IBasicAuthenticationError error)
        {
            if (error == null)
            {
                throw new ArgumentNullException("error");
            }

            _error = error;
        }

        public IPrincipal Principal
        {
            get { return _principal; }
        }

        public IBasicAuthenticationError ErrorResult
        {
            get { return _error; }
        }
    }
}
