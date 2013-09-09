// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens;
using System.Net.Http;
using System.Threading;
using Microsoft.Owin.Security.Jwt;

namespace Microsoft.Owin.Security.ActiveDirectory
{
    /// <summary>
    /// A security token provider which retrieves the issuer and signing tokens from a WSFed metadata endpoint.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "This type is only controlled through the interface, which is not disposable.")]
    internal class WsFedCachingSecurityTokenProvider : IIssuerSecurityTokenProvider
    {
        private readonly TimeSpan _refreshInterval = new TimeSpan(1, 0, 0, 0);

        private readonly ReaderWriterLockSlim _synclock = new ReaderWriterLockSlim();

        private readonly string _metadataEndpoint;

        private readonly TimeSpan _backchannelTimeout;
        private readonly HttpMessageHandler _backchannelHttpHandler;

        private DateTimeOffset _syncAfter = new DateTimeOffset(new DateTime(2001, 1, 1));

        private string _issuer;

        private IEnumerable<SecurityToken> _tokens;

        public WsFedCachingSecurityTokenProvider(string metadataEndpoint, ICertificateValidator backchannelCertificateValidator,
            TimeSpan backchannelTimeout, HttpMessageHandler backchannelHttpHandler)
        {
            _metadataEndpoint = metadataEndpoint;
            _backchannelTimeout = backchannelTimeout;
            _backchannelHttpHandler = backchannelHttpHandler ?? new WebRequestHandler();

            if (backchannelCertificateValidator != null)
            {
                // Set the cert validate callback
                var webRequestHandler = _backchannelHttpHandler as WebRequestHandler;
                if (webRequestHandler == null)
                {
                    throw new InvalidOperationException(Properties.Resources.Exception_ValidatorHandlerMismatch);
                }
                webRequestHandler.ServerCertificateValidationCallback = backchannelCertificateValidator.Validate;
            }

            RetrieveMetadata();
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
                _synclock.EnterReadLock();
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
                IssuerSigningKeys metaData = WsFedMetadataRetriever.GetSigningKeys(_metadataEndpoint,
                    _backchannelTimeout, _backchannelHttpHandler);
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
