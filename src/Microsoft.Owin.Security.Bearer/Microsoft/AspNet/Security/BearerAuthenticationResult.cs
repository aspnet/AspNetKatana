// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Security.Principal;

namespace Microsoft.AspNet.Security
{
    /// <summary></summary>
    public class BearerAuthenticationResult : IBearerAuthenticationResult
    {
        private readonly IBearerAuthenticationError _error;
        private readonly IPrincipal _principal;

        /// <summary></summary>
        /// <param name="principal"></param>
        public BearerAuthenticationResult(IPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException("principal");
            }

            _principal = principal;
        }

        /// <summary></summary>
        /// <param name="error"></param>
        public BearerAuthenticationResult(IBearerAuthenticationError error)
        {
            if (error == null)
            {
                throw new ArgumentNullException("error");
            }

            _error = error;
        }

        /// <inheritdoc />
        public IPrincipal Principal
        {
            get { return _principal; }
        }

        /// <inheritdoc />
        public IBearerAuthenticationError ErrorResult
        {
            get { return _error; }
        }
    }
}
