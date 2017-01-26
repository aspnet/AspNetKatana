// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security.Jwt;

namespace Microsoft.Owin.Security.ActiveDirectory
{
    /// <summary>
    /// A security key provider which retrieves the issuer and signing tokens from a WSFed metadata endpoint.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "This type is only controlled through the interface, which is not disposable.")]
    internal class WsFedCachingSecurityKeyProvider : IIssuerSecurityKeyProvider
    {
        private readonly TimeSpan _refreshInterval = new TimeSpan(1, 0, 0, 0);

        private readonly string _metadataEndpoint;

        private readonly TimeSpan _backchannelTimeout;
        private readonly HttpMessageHandler _backchannelHttpHandler;

        private DateTimeOffset _syncAfter = new DateTimeOffset(new DateTime(2001, 1, 1));

        private string _issuer;

        private IEnumerable<SecurityKey> _keys;

        public WsFedCachingSecurityKeyProvider(string metadataEndpoint, ICertificateValidator backchannelCertificateValidator,
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
                RefreshMetadata();
                return _issuer;
            }
        }

        /// <summary>
        /// Gets all known security keys.
        /// </summary>
        /// <value>
        /// All known security keys.
        /// </value>
        public IEnumerable<SecurityKey> SecurityKeys
        {
            get
            {
                RefreshMetadata();
                return _keys;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Can't throw exceptions on a background thread.")]
        private void RefreshMetadata()
        {
            if (_syncAfter >= DateTimeOffset.UtcNow)
            {
                return;
            }

            // Queue a refresh, but discourage other threads from doing so.
            _syncAfter = DateTimeOffset.UtcNow + _refreshInterval;
            ThreadPool.UnsafeQueueUserWorkItem(state =>
            {
                try
                {
                    RetrieveMetadata();
                }
                catch (Exception)
                {
                    // Don't throw exceptions on background threads.
                }
            }, state: null);
        }

        private void RetrieveMetadata()
        {
            _syncAfter = DateTimeOffset.UtcNow + _refreshInterval;
            IssuerSigningKeys metaData = WsFedMetadataRetriever.GetSigningKeys(_metadataEndpoint,
                _backchannelTimeout, _backchannelHttpHandler);
            _issuer = metaData.Issuer;
            _keys = metaData.Keys;
        }
    }
}
