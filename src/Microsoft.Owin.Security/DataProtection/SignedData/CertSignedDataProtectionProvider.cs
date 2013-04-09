// <copyright file="CertSignedDataProtectionProvider.cs" company="Microsoft Open Technologies, Inc.">
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

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.Owin.Security.DataProtection.SignedData
{
    /// <summary>
    /// Used to produce and verify X509 signatures within protected data.
    /// </summary>
    public class CertSignedDataProtectionProvider : IDataProtectionProvider
    {
        private readonly X509Certificate2 _cert;
        private readonly IDataProtectionProvider _provider;

        /// <summary>
        /// Used to produce and verify X509 signatures within protected data. The signature is appended to the user data, and both
        /// are protected by the given data protection provider.
        /// </summary>
        /// <param name="cert">The certificate which holds the public key used by Unprotect to verify data signatures. If the
        /// certificate holds the private key then it will be used by Protect to generate the data signature.</param>
        /// <param name="provider">The data protection provider which protects and unprotects the combination of user data and
        /// signature.</param>
        public CertSignedDataProtectionProvider(X509Certificate2 cert, IDataProtectionProvider provider)
        {
            _cert = cert;
            _provider = provider;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="purposes"></param>
        /// <returns></returns>
        public IDataProtection Create(params string[] purposes)
        {
            if (_cert.GetKeyAlgorithm() == "RSA")
            {
                if (_cert.HasPrivateKey)
                {
                    return new RsaSignedDataProtection((RSACryptoServiceProvider)_cert.PrivateKey, _provider.Create(purposes));
                }
                return new RsaSignedDataProtection((RSACryptoServiceProvider)_cert.PublicKey.Key, _provider.Create(purposes));
            }
            if (_cert.GetKeyAlgorithm() == "DSA")
            {
                if (_cert.HasPrivateKey)
                {
                    return new DsaSignedDataProtection((DSACryptoServiceProvider)_cert.PrivateKey, _provider.Create(purposes));
                }
                return new DsaSignedDataProtection((DSACryptoServiceProvider)_cert.PublicKey.Key, _provider.Create(purposes));
            }
            throw new CryptographicUnexpectedOperationException();
        }
    }
}
