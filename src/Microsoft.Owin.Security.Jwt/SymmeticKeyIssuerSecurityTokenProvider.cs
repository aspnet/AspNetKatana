// <copyright file="ISigningCredentialsProvider.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.ServiceModel.Security.Tokens;

namespace Microsoft.Owin.Security.Jwt
{
    /// <summary>
    /// Implements an <see cref="IIssuerSecurityTokenProvider"/> for symmetric key signed JWT tokens.
    /// </summary>
    public class SymmeticKeyIssuerSecurityTokenProvider : IIssuerSecurityTokenProvider
    {
        private readonly List<SecurityToken> _tokens = new List<SecurityToken>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SymmeticKeyIssuerSecurityTokenProvider"/> class.
        /// </summary>
        /// <param name="issuer">The issuer of a JWT token.</param>
        /// <param name="key">The symmetric key a JWT is signed with.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the issuer is null.</exception>
        public SymmeticKeyIssuerSecurityTokenProvider(string issuer, byte[] key)
            : this(issuer, new[] { key })
        {            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SymmeticKeyIssuerSecurityTokenProvider"/> class.
        /// </summary>
        /// <param name="issuer">The issuer of a JWT token.</param>
        /// <param name="keys">Symmetric keys a JWT could be signed with.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the issuer is null.</exception>
        public SymmeticKeyIssuerSecurityTokenProvider(string issuer, IEnumerable<byte[]> keys)
        {
            if (string.IsNullOrWhiteSpace(issuer))
            {
                throw new ArgumentNullException("issuer");
            }

            Issuer = issuer;
            foreach (var key in keys)
            {
                _tokens.Add(new BinarySecretSecurityToken(key));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SymmeticKeyIssuerSecurityTokenProvider"/> class.
        /// </summary>
        /// <param name="issuer">The issuer of a JWT token.</param>
        /// <param name="base64Key">The base64 encoded symmetric key a JWT is signed with.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the issuer is null.</exception>
        public SymmeticKeyIssuerSecurityTokenProvider(string issuer, string base64Key)
            : this(issuer, new[] { base64Key })
        {            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SymmeticKeyIssuerSecurityTokenProvider"/> class.
        /// </summary>
        /// <param name="issuer">The issuer of a JWT token.</param>
        /// <param name="base64Keys">The base64 encoded symmetric keys a JWT could be signed with.</param>
        public SymmeticKeyIssuerSecurityTokenProvider(string issuer, IEnumerable<string> base64Keys)
        {
            if (string.IsNullOrWhiteSpace(issuer))
            {
                throw new ArgumentNullException("issuer");
            }

            Issuer = issuer;
            foreach (var key in base64Keys)
            {
                _tokens.Add(new BinarySecretSecurityToken(Convert.FromBase64String(key)));
            }
        }

        /// <summary>
        /// Gets the issuer the signing keys are for.
        /// </summary>
        /// <value>
        /// The issuer the signing keys are for.
        /// </value>
        public string Issuer
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets all known security tokens.
        /// </summary>
        /// <returns>
        /// All known security tokens.
        /// </returns>
        public IEnumerable<SecurityToken> SecurityTokens
        {
            get
            {
                return _tokens.AsReadOnly();
            }
        }
    }
}
