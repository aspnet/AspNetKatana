// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
        public string Issuer { get; private set; }

        /// <summary>
        /// Gets all known security tokens.
        /// </summary>
        /// <value>
        /// All known security tokens.
        /// </value>
        public IEnumerable<SecurityToken> SecurityTokens
        {
            get { return _tokens.AsReadOnly(); }
        }
    }
}
