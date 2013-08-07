// <copyright file="X509CertificateSecurityTokenProvider.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.Owin.Security.Jwt
{
    /// <summary>
    /// Implements an <see cref="IIssuerSecurityTokenProvider"/> for X509 JWTs.
    /// </summary>
    public class X509CertificateSecurityTokenProvider : IIssuerSecurityTokenProvider
    {
        private readonly List<SecurityToken> _tokens = new List<SecurityToken>();

        /// <summary>
        /// Initializes a new instance of the <see cref="X509CertificateSecurityTokenProvider"/> class.
        /// </summary>
        /// <param name="issuer">The issuer.</param>
        /// <param name="certificate">The certificate.</param>
        /// <exception cref="System.ArgumentNullException">
        /// issuer
        /// or
        /// certificate
        /// </exception>
        public X509CertificateSecurityTokenProvider(string issuer, X509Certificate2 certificate)
        {
            if (string.IsNullOrWhiteSpace(issuer))
            {
                throw new ArgumentNullException("issuer");
            }

            if (certificate == null)
            {
                throw new ArgumentNullException("certificate");
            }

            Issuer = issuer;
            
            _tokens.Add(new X509SecurityToken(certificate));
        }

        /// <summary>
        /// Gets the issuer the credentials are for.
        /// </summary>
        /// <value>
        /// The issuer the credentials are for.
        /// </value>
        public string Issuer
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets all known security tokens.
        /// </summary>
        /// <value>
        /// All known security tokens.
        /// </value>
        public IEnumerable<SecurityToken> SecurityTokens
        {
            get
            {
                return _tokens.AsReadOnly();
            }
        }
    }
}
