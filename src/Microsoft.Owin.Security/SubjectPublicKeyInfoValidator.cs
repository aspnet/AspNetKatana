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
using System.ComponentModel;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using Microsoft.Win32;

namespace Microsoft.Owin.Security
{
    /// <summary>
    /// Implements a cert pinning validator passed on 
    /// http://datatracker.ietf.org/doc/draft-ietf-websec-key-pinning/?include_text=1
    /// </summary>
    public class SubjectPublicKeyInfoValidator : ICertificateValidator
    {
        private readonly HashSet<string> _validBase64EncodedSubjectPublicKeyInfoHashes;

        private SubjectPublicKeyInfoAlgorithm _algorithm;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubjectPublicKeyInfoValidator"/> class.
        /// </summary>
        /// <param name="validBase64EncodedSubjectPublicKeyInfoHashes">A collection of valid base64 encoded hashes of the certificate public key information blob.</param>
        /// <param name="algorithm">The algorithm used to generate the hashes.</param>
        public SubjectPublicKeyInfoValidator(IEnumerable<string> validBase64EncodedSubjectPublicKeyInfoHashes, SubjectPublicKeyInfoAlgorithm algorithm)
        {            
            _validBase64EncodedSubjectPublicKeyInfoHashes = new HashSet<string>(validBase64EncodedSubjectPublicKeyInfoHashes);

            if (_algorithm != SubjectPublicKeyInfoAlgorithm.Sha1 && _algorithm != SubjectPublicKeyInfoAlgorithm.Sha256)
            {
                throw new ArgumentOutOfRangeException("algorithm");
            }

            _algorithm = algorithm;
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
                return RemoteCertificateValidationCallbackValidator;
            }
        }

        /// <summary>
        /// Validates at least one SPKI hash is known.
        /// </summary>
        /// <param name="sender">An object that contains state information for this validation.</param>
        /// <param name="certificate">The certificate used to authenticate the remote party.</param>
        /// <param name="chain">The chain of certificate authorities associated with the remote certificate.</param>
        /// <param name="sslPolicyErrors">One or more errors associated with the remote certificate.</param>
        /// <returns>A Boolean value that determines whether the specified certificate is accepted for authentication.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "FXCop is confused.")]
        private bool RemoteCertificateValidationCallbackValidator(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors != SslPolicyErrors.None)
            {
                return false;
            }

            if (chain.ChainElements.Count < 2)
            {
                return false;
            }

            bool pinnedCertificateDiscovered = false;
            using (HashAlgorithm algorithm = _algorithm == SubjectPublicKeyInfoAlgorithm.Sha1 ? (HashAlgorithm)new SHA1CryptoServiceProvider() : new SHA256CryptoServiceProvider())
            {
                foreach (var chainElement in chain.ChainElements)
                {                    
                    X509Certificate2 chainedCertificate = chainElement.Certificate;
                    var base64Spki = Convert.ToBase64String(algorithm.ComputeHash(ExtractSpkiBlob(chainedCertificate)));
                    if (_validBase64EncodedSubjectPublicKeyInfoHashes.Contains(base64Spki))
                    {
                        pinnedCertificateDiscovered = true;
                    }
                }
            }

            return pinnedCertificateDiscovered;
        }

        private static byte[] ExtractSpkiBlob(X509Certificate2 certificate)
        {
            // Get a native cert_context from the managed X590Certificate2 instance.
            NativeMethods.CERT_CONTEXT certContext = (NativeMethods.CERT_CONTEXT)Marshal.PtrToStructure(certificate.Handle, typeof(NativeMethods.CERT_CONTEXT));
            
            // Pull the CERT_INFO structure from the context.
            NativeMethods.CERT_INFO certInfo = (NativeMethods.CERT_INFO)Marshal.PtrToStructure(certContext.pCertInfo, typeof(NativeMethods.CERT_INFO));
            
            // And finally grab the key information, public key, algorithm and parameters from it.
            NativeMethods.CERT_PUBLIC_KEY_INFO publicKeyInfo = certInfo.SubjectPublicKeyInfo;

            // Now start encoding to ASN1.
            // First see how large the ASN1 representation is going to be.
            UInt32 blobSize = 0;
            var structType = new IntPtr(NativeMethods.X509_PUBLIC_KEY_INFO);
            if (!NativeMethods.CryptEncodeObject(NativeMethods.X509_ASN_ENCODING, structType, ref publicKeyInfo, null, ref blobSize))
            {
                int error = Marshal.GetLastWin32Error();
                throw new Win32Exception(error);
            }

            // Allocate enough space.
            var blob = new byte[blobSize];

            // Finally get the ASN1 representation.
            if (!NativeMethods.CryptEncodeObject(NativeMethods.X509_ASN_ENCODING, structType, ref publicKeyInfo, blob, ref blobSize))
            {
                int error = Marshal.GetLastWin32Error();
                throw new Win32Exception(error);
            }

            return blob;
        }
    }
}
