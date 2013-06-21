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

using System.Collections.Generic;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.Owin.Security
{
    /// <summary>
    /// Provides pinned certificate validation based on the certificate thumbprint.
    /// </summary>
    public class CertificateThumbprintValidator : IPinnedCertificateValidator
    {
        private readonly HashSet<string> _validCertificateThumbprints;

        /// <summary>
        /// Initializes a new instance of the <see cref="CertificateThumbprintValidator"/> class.
        /// </summary>
        /// <param name="validThumbprints">A set of thumbprints which are valid for an HTTPS request.</param>
        public CertificateThumbprintValidator(IEnumerable<string> validThumbprints)
        {
            _validCertificateThumbprints = new HashSet<string>(validThumbprints);
        }

        /// <summary>
        /// Gets the function used to validate HTTPS certificates.
        /// </summary>
        /// <value>
        /// The function used to validate HTTPS certificates.
        /// </value>
        public RemoteCertificateValidationCallback RemoteCertificateValidationCallback
        {
            get
            {
                return this.RemoteCertificateValidationCallbackValidator;
            }
        }

        /// <summary>
        /// Validates that the certificate thumbprints in the signing chain of an HTTPS call match 
        /// those contained in a list of valid thumbprints.
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

            if ((_validCertificateThumbprints == null || _validCertificateThumbprints.Count == 0))
            {
                return true;
            }

            int position = 0;
            foreach (var chainElement in chain.ChainElements)
            {
                if (position != 0)
                {
                    if (chainElement.Certificate.Thumbprint == null)
                    {
                        return false;
                    }

                    if (!_validCertificateThumbprints.Contains(chainElement.Certificate.Thumbprint))
                    {
                        return false;
                    }
                }

                position++;
            }

            return true;
        }
    }
}
