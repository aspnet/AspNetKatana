// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.Owin.Security.Jwt
{
    /// <summary>
    /// Implements an <see cref="IIssuerSecurityKeyProvider"/> for X509 JWTs.
    /// </summary>
    public class X509CertificateSecurityKeyProvider : IIssuerSecurityKeyProvider
    {
        private readonly List<SecurityKey> _keys = new List<SecurityKey>();

        /// <summary>
        /// Initializes a new instance of the <see cref="X509CertificateSecurityKeyProvider"/> class.
        /// </summary>
        /// <param name="issuer">The issuer.</param>
        /// <param name="certificate">The certificate.</param>
        /// <exception cref="System.ArgumentNullException">
        /// issuer
        /// or
        /// certificate
        /// </exception>
        public X509CertificateSecurityKeyProvider(string issuer, X509Certificate2 certificate)
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

            _keys.Add(new X509SecurityKey(certificate));
        }

        /// <summary>
        /// Gets the issuer the credentials are for.
        /// </summary>
        /// <value>
        /// The issuer the credentials are for.
        /// </value>
        public string Issuer { get; private set; }

        /// <summary>
        /// Gets all known security keys.
        /// </summary>
        /// <value>
        /// All known security keys.
        /// </value>
        public IEnumerable<SecurityKey> SecurityKeys
        {
            get { return _keys.AsReadOnly(); }
        }
    }
}
