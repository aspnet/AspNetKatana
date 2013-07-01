// <copyright file="CertificateThumbprintValidator.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.Owin.Security
{
    /// <summary>
    /// Provides pinned certificate validation based on the certificate thumbprint.
    /// </summary>
    public class CertificateThumbprintValidator : ICertificateValidator
    {
        private readonly HashSet<string> _validCertificateThumbprints;

        /// <summary>
        /// Initializes a new instance of the <see cref="CertificateThumbprintValidator"/> class.
        /// </summary>
        /// <param name="validThumbprints">A set of thumbprints which are valid for an HTTPS request.</param>
        public CertificateThumbprintValidator(IEnumerable<string> validThumbprints)
        {
            if (validThumbprints == null)
            {
                throw new ArgumentNullException("validThumbprints");
            }

            _validCertificateThumbprints = new HashSet<string>(validThumbprints, StringComparer.OrdinalIgnoreCase);

            if (_validCertificateThumbprints.Count == 0)
            {
                throw new ArgumentOutOfRangeException("validThumbprints");
            }
        }

        /// <summary>
        /// Validates that the certificate thumbprints in the signing chain match at least one whitelisted thumbprint.
        /// </summary>
        /// <param name="sender">An object that contains state information for this validation.</param>
        /// <param name="certificate">The certificate used to authenticate the remote party.</param>
        /// <param name="chain">The chain of certificate authorities associated with the remote certificate.</param>
        /// <param name="sslPolicyErrors">One or more errors associated with the remote certificate.</param>
        /// <returns>A Boolean value that determines whether the specified certificate is accepted for authentication.</returns>
        public bool Validate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors != SslPolicyErrors.None)
            {
                return false;
            }

            if (chain == null)
            {
                throw new ArgumentNullException("chain");
            }

            if (chain.ChainElements.Count < 2)
            {
                // Self signed.
                return false;
            }

            foreach (X509ChainElement chainElement in chain.ChainElements)
            {
                string thumbprintToCheck = chainElement.Certificate.Thumbprint;

                if (thumbprintToCheck == null)
                {
                    continue;
                }

                if (_validCertificateThumbprints.Contains(thumbprintToCheck))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
