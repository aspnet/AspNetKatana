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
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.Owin.Security
{
    /// <summary>
    /// The algorithm used to generate the subject public key information blob hashes.
    /// </summary>
    public enum ThumbprintGenerationAlgorithm
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sha", Justification = "It is correct.")]
        Sha1,
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sha", Justification = "It is correct.")]
        Sha256
    }

    /// <summary>
    /// Implements a cert pinning validator passed on 
    /// http://datatracker.ietf.org/doc/draft-ietf-websec-key-pinning/?include_text=1
    /// </summary>
    public class SubjectPublicKeyInfoValidator : IPinnedCertificateValidator
    {
        private readonly HashSet<string> _validBase64EncodedSubjectPublicKeyInfoHashes;

        private ThumbprintGenerationAlgorithm _algorithm;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubjectPublicKeyInfoValidator"/> class.
        /// </summary>
        /// <param name="validBase64EncodedSubjectPublicKeyInfoHashes">A collection of valid base64 encoded hashes of the certificate public key information blob.</param>
        /// <param name="algorithm">The algorithm used to generate the hashes.</param>
        public SubjectPublicKeyInfoValidator(IEnumerable<string> validBase64EncodedSubjectPublicKeyInfoHashes, ThumbprintGenerationAlgorithm algorithm)
        {            
            this._validBase64EncodedSubjectPublicKeyInfoHashes = new HashSet<string>(validBase64EncodedSubjectPublicKeyInfoHashes);
            this._algorithm = algorithm;
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
                return false;
            }

            HashAlgorithm algorithm;

            if (_algorithm == ThumbprintGenerationAlgorithm.Sha1)
            {
                algorithm = new SHA1CryptoServiceProvider();
            }
            else
            {
                algorithm = new SHA256CryptoServiceProvider();                
            }

            try
            {
                int position = 0;
                foreach (var chainElement in chain.ChainElements)
                {
                    if (position == 0)
                    {
                        position++;
                        continue;
                    }

                    var chainedCertificate = chainElement.Certificate;

                    var certContext = (CERT_CONTEXT)Marshal.PtrToStructure(chainedCertificate.Handle, typeof(CERT_CONTEXT));
                    var certInfo = (CERT_INFO)Marshal.PtrToStructure(certContext.pCertInfo, typeof(CERT_INFO));
                    var publicKeyInfo = certInfo.SubjectPublicKeyInfo;

                    UInt32 blobSize = 0;
                    byte[] blob = null;
                    var structType = new IntPtr(NativeMethods.X509_PUBLIC_KEY_INFO);

                    if (!NativeMethods.CryptEncodeObject(NativeMethods.X509_ASN_ENCODING, structType, ref publicKeyInfo, null, ref blobSize))
                    {
                        int error = Marshal.GetLastWin32Error();
                        throw new Win32Exception(error);
                    }

                    blob = new byte[blobSize];
                    if (!NativeMethods.CryptEncodeObject(NativeMethods.X509_ASN_ENCODING, structType, ref publicKeyInfo, blob, ref blobSize))
                    {
                        int error = Marshal.GetLastWin32Error();
                        throw new Win32Exception(error);
                    }

                    var base64Spki = Convert.ToBase64String(algorithm.ComputeHash(blob));

                    if (!this._validBase64EncodedSubjectPublicKeyInfoHashes.Contains(base64Spki))
                    {
                        return false;
                    }

                    position++;
                }
            }
            finally
            {
                algorithm.Dispose();
            }

            return true;
        }

        // ReSharper disable InconsistentNaming
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct CRYPT_BLOB
        {
            public Int32 cbData;
            public IntPtr pbData;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct CERT_CONTEXT
        {
            public Int32 dwCertEncodingType;
            public IntPtr pbCertEncoded;
            public Int32 cbCertEncoded;
            public IntPtr pCertInfo;
            public IntPtr hCertStore;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class CERT_INFO
        {
            public Int32 dwVersion;
            public CRYPT_BLOB SerialNumber;
            public CRYPT_ALGORITHM_IDENTIFIER SignatureAlgorithm;
            public CRYPT_BLOB Issuer;
            public System.Runtime.InteropServices.ComTypes.FILETIME NotBefore;
            public System.Runtime.InteropServices.ComTypes.FILETIME NotAfter;
            public CRYPT_BLOB Subject;
            public CERT_PUBLIC_KEY_INFO SubjectPublicKeyInfo;
            public CRYPT_BIT_BLOB IssuerUniqueId;
            public CRYPT_BIT_BLOB SubjectUniqueId;
            public Int32 cExtension;
            public IntPtr rgExtension;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        internal struct CRYPT_ALGORITHM_IDENTIFIER
        {
            public string pszObjId;
            public CRYPT_BLOB Parameters;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        internal struct CRYPT_BIT_BLOB
        {
            public Int32 cbData;
            public IntPtr pbData;
            public Int32 cUnusedBits;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct CERT_PUBLIC_KEY_INFO
        {
            public CRYPT_ALGORITHM_IDENTIFIER Algorithm;
            public CRYPT_BIT_BLOB PublicKey;
        }
        // ReSharper restore InconsistentNaming

        [SuppressUnmanagedCodeSecurity]
        internal static class NativeMethods
        {
            // ReSharper disable InconsistentNaming
            public const int X509_ASN_ENCODING = 0x00000001;
            public const int X509_PUBLIC_KEY_INFO = 8;
            // ReSharper restore InconsistentNaming

            [DllImport("crypt32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool CryptEncodeObject(
                UInt32 dwCertEncodingType,
                IntPtr lpszStructType,
                ref CERT_PUBLIC_KEY_INFO pvStructInfo,
                byte[] pbEncoded,
                ref UInt32 pcbEncoded);
        }
    }
}
