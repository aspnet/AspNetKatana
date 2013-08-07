// <copyright file="WsFedCachingSecurityTokenProvider.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Threading;

using Microsoft.Owin.Security.Jwt;

namespace Microsoft.Owin.Security.ActiveDirectoryFederationServices
{
    /// <summary>
    /// A security token provider which retrieves the issuer and signing tokens from a WSFed metadata endpoint.
    /// </summary>
    internal class WsFedCachingSecurityTokenProvider : IIssuerSecurityTokenProvider
    {
        private readonly TimeSpan _refreshInterval = new TimeSpan(1, 0, 0, 0);

        private readonly ReaderWriterLockSlim _synclock = new ReaderWriterLockSlim();

        private readonly string _metadataEndpoint;

        private readonly bool _validateMetadataEndpointCertificate;

        private DateTimeOffset _syncAfter = new DateTimeOffset(new DateTime(2001, 1, 1));

        private string _issuer;

        private IEnumerable<SecurityToken> _tokens;

        /// <summary>
        /// Initializes a new instance of the <see cref="WsFedCachingSecurityTokenProvider"/> class.
        /// </summary>
        /// <param name="metadataEndpoint">The metadata endpoint.</param>
        /// <param name="validateMetadataEndpointCertificate">If set to false the certificate on the metadata endpoint will not be validated.</param>
        public WsFedCachingSecurityTokenProvider(string metadataEndpoint, bool validateMetadataEndpointCertificate)
        {
            _metadataEndpoint = metadataEndpoint;
            _validateMetadataEndpointCertificate = validateMetadataEndpointCertificate;

            RetrieveMetadata();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WsFedCachingSecurityTokenProvider"/> class.
        /// </summary>
        /// <param name="metadataEndpoint">The metadata endpoint.</param>
        /// <param name="validateMetadataEndpointCertificate">If set to false the certificate on the metadata endpoint will not be validated.</param>
        /// <param name="refreshInterval">The refresh interval after which the signing information will be refreshed from the metadata endpoint.</param>
        public WsFedCachingSecurityTokenProvider(string metadataEndpoint, bool validateMetadataEndpointCertificate, TimeSpan refreshInterval) : this(metadataEndpoint, validateMetadataEndpointCertificate)
        {
            _refreshInterval = refreshInterval;
        }

        /// <summary>
        /// Gets the issuer the credentials are for.
        /// </summary>
        /// <value>
        /// The issuer the credentials are for.
        /// </value>
        public string Issuer
        {
            get
            {
                RetrieveMetadata();
                try
                {
                    return _issuer;
                }
                finally
                {
                    _synclock.ExitReadLock();
                }
            }
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
                RetrieveMetadata();
                _synclock.EnterReadLock();
                try
                {
                    return _tokens;
                }
                finally
                {
                    _synclock.ExitReadLock();
                }                
            }
        }

        private void RetrieveMetadata()
        {
            if (_syncAfter >= DateTimeOffset.UtcNow)
            {
                return;
            }

            _synclock.EnterWriteLock();
            try
            {
                var metaData = WsFedMetadataRetriver.GetSigningKeys(_metadataEndpoint, _validateMetadataEndpointCertificate);
                _issuer = metaData.Issuer;
                _tokens = metaData.Tokens;
                _syncAfter = DateTimeOffset.UtcNow + _refreshInterval;
            }
            finally
            {
                _synclock.ExitWriteLock();
            }
        }
    }
}
