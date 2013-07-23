// <copyright file="SelfSigningTokenProvider.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Linq;
using System.Security.Cryptography;
using System.ServiceModel.Security.Tokens;
using System.Threading;

using Microsoft.Owin.Infrastructure;

namespace Microsoft.Owin.Security.Jwt
{
    /// <summary>
    /// Implements a provider for self signed JWT, where an application 
    /// issues its own JWT for self consumption.
    /// </summary>
    public class SelfSigningJwtProvider : ISigningCredentialsProvider
    {
        private readonly TimeSpan _keyExpiry;
        private readonly List<string> _audiences = new List<string>();
        private readonly Dictionary<Guid, GeneratedSymmetricCredentials> _signingKeys = new Dictionary<Guid, GeneratedSymmetricCredentials>();
        private readonly ReaderWriterLockSlim _syncLock = new ReaderWriterLockSlim();

        private Guid _currentKeyId = Guid.Empty;        

        /// <summary>
        /// Initializes a new instance of the <see cref="SelfSigningJwtProvider"/> class.
        /// </summary>
        /// <param name="issuer">The issuer for the JWT.</param>
        public SelfSigningJwtProvider(string issuer)
        {
            if (string.IsNullOrWhiteSpace(issuer))
            {
                throw new ArgumentNullException("issuer");
            }

            Issuer = issuer;

            _keyExpiry = new TimeSpan(4, 0, 0);

            _audiences.Add(issuer);

            SystemClock = new SystemClock();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SelfSigningJwtProvider"/> class.
        /// </summary>
        /// <param name="issuer">The issuer for the JWT.</param>
        /// <param name="rotateCredentialsAfter">The time span a signing key is valid for.</param>
        public SelfSigningJwtProvider(string issuer, TimeSpan rotateCredentialsAfter) : this(issuer)
        {
            _keyExpiry = rotateCredentialsAfter;
        }

        /// <summary>
        /// Gets the JWT issuer.
        /// </summary>
        /// <value>
        /// The JWT issuer.
        /// </value>
        public string Issuer
        {
            get;
            private set;
        }

        /// <summary>
        /// The SystemClock provides access to the system's current time coordinates. If it is not provided a default instance is
        /// used which calls DateTimeOffset.UtcNow. This is typically not replaced except for unit testing. 
        /// </summary>
        public ISystemClock SystemClock
        {
            get; 
            set;
        }

        /// <summary>
        /// Gets the credentials used to sign the JWT.
        /// </summary>
        /// <value>
        /// The credentials used to sign the JWT.
        /// </value>
        public SigningCredentials SigningCredentials
        {
            get
            {
                if (_currentKeyId == Guid.Empty ||
                    _signingKeys[_currentKeyId].ExpiresOn < SystemClock.UtcNow)
                {
                    RotateKey();
                }

                byte[] currentKey;
                string keyIdentifier;

                _syncLock.EnterReadLock();
                try
                {
                    keyIdentifier = _currentKeyId.ToString("D");
                    currentKey = _signingKeys[_currentKeyId].Key;
                }
                finally
                {
                    _syncLock.ExitReadLock();
                }
                
                var keyIdentifierClause = new NamedKeySecurityKeyIdentifierClause("kid", keyIdentifier);
                var securityKeyIdentifer = new SecurityKeyIdentifier(new SecurityKeyIdentifierClause[] { keyIdentifierClause });
                var signingCredentials = new SigningCredentials(new InMemorySymmetricSecurityKey(currentKey), SecurityAlgorithms.HmacSha256Signature, SecurityAlgorithms.Sha256Digest, securityKeyIdentifer);
                return signingCredentials;
            }
        }

        /// <summary>
        /// Gets the expected security token for the specified <paramref name="identifier" /> for use in signature validation.
        /// </summary>
        /// <param name="identifier">The token identifier.</param>
        /// <returns>
        /// The security token identified by <paramref name="identifier" />.
        /// </returns>
        public SecurityToken GetSecurityTokenForKeyIdentifier(string identifier)
        {
            return new BinarySecretSecurityToken(_signingKeys[new Guid(identifier)].Key);
        }

        /// <summary>
        /// Gets all known security tokens.
        /// </summary>
        /// <returns>
        /// All known security tokens.
        /// </returns>
        public IEnumerable<SecurityToken> GetSecurityTokens()
        {
            return _signingKeys.Select(signingKey => new BinarySecretSecurityToken(signingKey.Value.Key));
        }

        private void RotateKey()
        {
            var keyIdentifer = Guid.NewGuid();
            var signingAlgorithm = new AesManaged();
            var signingCredentials = new GeneratedSymmetricCredentials(signingAlgorithm.Key, _keyExpiry, SystemClock);

            _syncLock.EnterWriteLock();
            try
            {
                _signingKeys.Add(keyIdentifer, signingCredentials);
                _currentKeyId = keyIdentifer;

                if (_signingKeys.Count <= 5)
                {
                    return;
                }

                var oldestKeyIdentifier = _signingKeys.OrderBy(k => k.Value.ExpiresOn).First().Key;
                _signingKeys.Remove(oldestKeyIdentifier);
            }
            finally
            {
                _syncLock.ExitWriteLock();
            }
        }

        private class GeneratedSymmetricCredentials
        {
            public GeneratedSymmetricCredentials(byte[] key, TimeSpan expiresAfter, ISystemClock clock)
            {
                Key = key;
                ExpiresOn = clock.UtcNow.Add(expiresAfter);
            }

            public DateTimeOffset ExpiresOn
            {
                get;
                private set;
            }

            public byte[] Key
            {
                get;
                private set;
            }
        }
    }
}
