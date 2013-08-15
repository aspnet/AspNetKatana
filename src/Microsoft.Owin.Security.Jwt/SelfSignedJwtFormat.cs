// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;

namespace Microsoft.Owin.Security.Jwt
{
    /// <summary>
    /// Signs and validates JSON Web Tokens for consumption by the issuing application.
    /// </summary>
    public class SelfSignedJwtFormat : JwtFormat
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SelfSignedJwtFormat"/> class.
        /// </summary>
        /// <param name="issuer">The JWT issuer.</param>
        public SelfSignedJwtFormat(string issuer) : base(new SelfSigningJwtProvider(issuer))
        {
            ValidateIssuer = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SelfSignedJwtFormat"/> class.
        /// </summary>
        /// <param name="issuer">The JWT issuer.</param>
        /// <param name="rotateCredentialsAfter">The time span a signing key is valid for.</param>
        public SelfSignedJwtFormat(string issuer, TimeSpan rotateCredentialsAfter) : base(new SelfSigningJwtProvider(issuer, rotateCredentialsAfter))
        {
            ValidateIssuer = true;
        }
    }
}
