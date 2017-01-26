// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.Owin.Security.Jwt
{
    /// <summary>
    /// Implements an <see cref="IIssuerSecurityKeyProvider"/> for symmetric key signed JWTs.
    /// </summary>
    public class SymmetricKeyIssuerSecurityKeyProvider : IIssuerSecurityKeyProvider
    {
        private readonly List<SecurityKey> _keys = new List<SecurityKey>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SymmetricKeyIssuerSecurityKeyProvider"/> class.
        /// </summary>
        /// <param name="issuer">The issuer of a JWT token.</param>
        /// <param name="key">The symmetric key a JWT is signed with.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the issuer is null.</exception>
        public SymmetricKeyIssuerSecurityKeyProvider(string issuer, byte[] key)
            : this(issuer, new[] { key })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SymmetricKeyIssuerSecurityKeyProvider"/> class.
        /// </summary>
        /// <param name="issuer">The issuer of a JWT token.</param>
        /// <param name="keys">Symmetric keys a JWT could be signed with.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the issuer is null.</exception>
        public SymmetricKeyIssuerSecurityKeyProvider(string issuer, IEnumerable<byte[]> keys)
        {
            if (string.IsNullOrWhiteSpace(issuer))
            {
                throw new ArgumentNullException("issuer");
            }
            if (keys == null)
            {
                throw new ArgumentNullException("keys");
            }

            Issuer = issuer;
            foreach (var key in keys)
            {
                _keys.Add(new SymmetricSecurityKey(key));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SymmetricKeyIssuerSecurityKeyProvider"/> class.
        /// </summary>
        /// <param name="issuer">The issuer of a JWT token.</param>
        /// <param name="base64Key">The base64 encoded symmetric key a JWT is signed with.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the issuer is null.</exception>
        public SymmetricKeyIssuerSecurityKeyProvider(string issuer, string base64Key)
            : this(issuer, new[] { base64Key })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SymmetricKeyIssuerSecurityKeyProvider"/> class.
        /// </summary>
        /// <param name="issuer">The issuer of a JWT token.</param>
        /// <param name="base64Keys">The base64 encoded symmetric keys a JWT could be signed with.</param>
        public SymmetricKeyIssuerSecurityKeyProvider(string issuer, IEnumerable<string> base64Keys)
        {
            if (string.IsNullOrWhiteSpace(issuer))
            {
                throw new ArgumentNullException("issuer");
            }
            if (base64Keys == null)
            {
                throw new ArgumentNullException("base64Keys");
            }

            Issuer = issuer;
            foreach (var key in base64Keys)
            {
                _keys.Add(new SymmetricSecurityKey(Convert.FromBase64String(key)));
            }
        }

        /// <summary>
        /// Gets the issuer the signing keys are for.
        /// </summary>
        /// <value>
        /// The issuer the signing keys are for.
        /// </value>
        public string Issuer { get; private set; }

        /// <summary>
        /// Gets all known security keys.
        /// </summary>
        /// <returns>
        /// All known security keys.
        /// </returns>
        public IEnumerable<SecurityKey> SecurityKeys
        {
            get { return _keys.AsReadOnly(); }
        }
    }
}
