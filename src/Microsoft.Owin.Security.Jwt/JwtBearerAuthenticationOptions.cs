// <copyright file="JwtBearerTokenOptions.cs" company="Microsoft Open Technologies, Inc.">
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

using System.Collections.Generic;

using Microsoft.Owin.Security.OAuth;

namespace Microsoft.Owin.Security.Jwt
{
    /// <summary>
    /// Options for JWT Bearer Token handler configuration.
    /// </summary>
    public class JwtBearerAuthenticationOptions : AuthenticationOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JwtBearerAuthenticationOptions"/> class.
        /// </summary>
        public JwtBearerAuthenticationOptions()
            : base(OAuthDefaults.AuthenticationType)
        {            
        }

        /// <summary>
        /// Gets or sets the allowed audiences an inbound JWT will be checked against.
        /// </summary>
        /// <value>
        /// The allowed audiences.
        /// </value>
        public IEnumerable<string> AllowedAudiences { get; set; }

        /// <summary>
        /// Gets or sets the issuer security token providers which provide the signing keys
        /// a JWT signature is checked against.
        /// </summary>
        /// <value>
        /// The issuer security token providers.
        /// </value>
        public IEnumerable<IIssuerSecurityTokenProvider> IssuerSecurityTokenProviders { get; set; }

        /// <summary>
        /// Gets or sets the signing credentials provider which provides the key necessary
        /// to issue a JSON Web Token.
        /// </summary>
        /// <value>
        /// The signing credentials provider.
        /// </value>
        /// <remarks>
        /// If this is not provided the JWT protector will only validate inbound JWTs.
        /// </remarks>
        public ISigningCredentialsProvider SigningCredentialsProvider { get; set; }

        /// <summary>
        /// Gets or sets the authentication provider.
        /// </summary>
        /// <value>
        /// The provider.
        /// </value>
        public IOAuthBearerAuthenticationProvider Provider { get; set; }

        /// <summary>
        /// Gets or sets the authentication realm.
        /// </summary>
        /// <value>
        /// The authentication realm.
        /// </value>
        public string Realm { get; set; }
    }
}
