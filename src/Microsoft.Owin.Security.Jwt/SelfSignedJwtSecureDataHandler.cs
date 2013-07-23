// <copyright file="SelfSignedJwtSecureDataHandler.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;

namespace Microsoft.Owin.Security.Jwt
{
    /// <summary>
    /// Signs and validates JSON Web Tokens for consumption by the issuing application.
    /// </summary>
    public class SelfSignedJwtSecureDataHandler : JwtSecureDataHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SelfSignedJwtSecureDataHandler"/> class.
        /// </summary>
        /// <param name="issuer">The JWT issuer.</param>
        public SelfSignedJwtSecureDataHandler(string issuer) : base(new SelfSigningJwtProvider(issuer))
        {            
            ValidateIssuer = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SelfSignedJwtSecureDataHandler"/> class.
        /// </summary>
        /// <param name="issuer">The JWT issuer.</param>
        /// <param name="rotateCredentialsAfter">The time span a signing key is valid for.</param>
        public SelfSignedJwtSecureDataHandler(string issuer, TimeSpan rotateCredentialsAfter) : base(new SelfSigningJwtProvider(issuer, rotateCredentialsAfter))
        {
            ValidateIssuer = true;
        }
    }
}
