// <copyright file="PinnedCertificateValidator.cs" company="Microsoft Open Technologies, Inc.">
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

using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.Owin.Security
{
    /// <summary>
    /// Provides pinned certificate validation, which checks HTTPS communication
    /// against a known good list of certificates to protect against compromised
    /// or rogue CAs issuing certificates for hosts without the knowledge of
    /// the host owner.
    /// </summary>
    public class PinnedCertificateValidator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PinnedCertificateValidator"/> class.
        /// </summary>
        /// <param name="validThumbprints">A HashSet of thumbprints which are valid for an HTTPS request.</param>
        public PinnedCertificateValidator(HashSet<string> validThumbprints) : this(validThumbprints, null)
        {            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PinnedCertificateValidator"/> class.
        /// </summary>
        /// <param name="validThumbprints">A HashSet of thumbprints which are valid for an HTTPS request.</param>
        /// <param name="invalidThumbprints">A HashSet of thumbprints which are invalid for an HTTPS request.</param>
        public PinnedCertificateValidator(HashSet<string> validThumbprints, HashSet<string> invalidThumbprints)
        {
            this.ValidCertificateThumbprints = validThumbprints;
            this.InvalidCertificateThumbprints = invalidThumbprints;
        }

        /// <summary>
        /// Gets or sets the valid thumbprints for back channel authentication.
        /// </summary>
        /// <value>
        /// The valid certificate thumbprints.
        /// </value>
        public HashSet<string> ValidCertificateThumbprints { get; private set; }

        /// <summary>
        /// Gets or sets the invalid thumbprints for back channel authentication.
        /// </summary>
        /// <value>
        /// The invalid certificate thumbprints.
        /// </value>
        public HashSet<string> InvalidCertificateThumbprints { get; private set; }

        /// <summary>
        /// Gets the remote certificate validation callback.
        /// </summary>
        /// <value>
        /// The remote certificate validation callback.
        /// </value>
        public RemoteCertificateValidationCallback RemoteCertificateValidationCallback
        {
            get
            {
                return this.RemoteCertificateValidationCallbackValidator;
            }
        }

        /// <summary>
        /// Validates that the certificate presented and its signing chain is not contained in the <see cref="InvalidCertificateThumbprints"/> and
        /// is contained in the <see cref="ValidCertificateThumbprints"/>.
        /// </summary>
        /// <param name="sender">An object that contains state information for this validation.</param>
        /// <param name="certificate">The certificate used to authenticate the remote party.</param>
        /// <param name="chain">The chain of certificate authorities associated with the remote certificate.</param>
        /// <param name="sslPolicyErrors">One or more errors associated with the remote certificate.</param>
        /// <returns>A Boolean value that determines whether the specified certificate is accepted for authentication.</returns>
        private bool RemoteCertificateValidationCallbackValidator(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors != SslPolicyErrors.None)
            {
                return false;
            }

            if (chain.ChainElements.Count < 2)
            {
                // Self signed.
                return false;
            }

            if ((ValidCertificateThumbprints == null || ValidCertificateThumbprints.Count == 0) &&
                (InvalidCertificateThumbprints == null || InvalidCertificateThumbprints.Count == 0))
            {
                return true;
            }

            if (InvalidCertificateThumbprints != null)
            {
                var presentedCertificate = new X509Certificate2(certificate);

                if (presentedCertificate.Thumbprint == null)
                {
                    return false;
                }

                if (InvalidCertificateThumbprints.Contains(presentedCertificate.Thumbprint))
                {
                    return false;
                }

                if (chain.ChainElements.Cast<X509ChainElement>().Any(chainElement => InvalidCertificateThumbprints.Contains(chainElement.Certificate.Thumbprint)))
                {
                    return false;
                }
            }

            if (ValidCertificateThumbprints != null)
            {
                int position = 0;
                foreach (var chainElement in chain.ChainElements)
                {
                    if (position != 0)
                    {
                        if (chainElement.Certificate.Thumbprint == null)
                        {
                            return false;
                        }

                        if (!ValidCertificateThumbprints.Contains(chainElement.Certificate.Thumbprint))
                        {
                            return false;
                        }
                    }

                    position++;
                }

                return true;
            }

            return false;
        }
    }
}
