// <copyright file="SharedSecretDataProtectionProvider.cs" company="Microsoft Open Technologies, Inc.">
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

using System.IO;
using System.Security.Cryptography;

namespace Microsoft.Owin.Security.DataProtection
{
    /// <summary>
    /// Used to provide the data protection services that are derived from a shared secret value. It may be used
    /// in web farm scenarios where neither the MachineKeyDataProtectionProvider nor the DpapiDataProtectionProvider are
    /// will produce the same results on different machines. 
    /// </summary>
    public class SharedSecretDataProtectionProvider : IDataProtectionProvider
    {
        private readonly string _sharedSecret;
        private readonly string _symmetricAlgorithmName;
        private readonly string _hashAlgorithmName;

        /// <summary>
        /// Initialize the provider instance.
        /// </summary>
        /// <param name="sharedSecret">The text used to derive the key material to encrypt and protect user data. This text
        /// should not be put in source control systems. Ideally the value used in production should only be available to the 
        /// web server software at runtime and to the administrators of the web server software.</param>
        /// <param name="symmetricAlgorithmName">The name of the symmetric algorithm providing data privacy, like AES</param>
        /// <param name="hashAlgorithmName">The name of the keyed hash algorithm providing data protection, like HMACSHA1</param>
        public SharedSecretDataProtectionProvider(string sharedSecret, string symmetricAlgorithmName, string hashAlgorithmName)
        {
            _sharedSecret = sharedSecret;
            _symmetricAlgorithmName = symmetricAlgorithmName;
            _hashAlgorithmName = hashAlgorithmName;
        }

        /// <summary>
        /// Returns a new instance of IDataProtection for the provider.
        /// </summary>
        /// <param name="purposes">Additional entropy used to ensure protected data may only be unprotected for the correct purposes.</param>
        /// <returns>An instance of a data protection service</returns>
        public IDataProtection Create(params string[] purposes)
        {
            var salt = new MemoryStream();
            using (var writer = new BinaryWriter(salt))
            {
                foreach (var purpose in purposes)
                {
                    writer.Write(purpose);
                }
                writer.Write("Microsoft.Owin.Security.DataProtection.SharedSecretDataProtectionProvider");
            }

            var deriveBytes = new Rfc2898DeriveBytes(_sharedSecret, salt.ToArray());

            SymmetricAlgorithm symmetricAlgorithm = SymmetricAlgorithm.Create(_symmetricAlgorithmName);
            symmetricAlgorithm.Key = deriveBytes.GetBytes(symmetricAlgorithm.Key.Length);

            KeyedHashAlgorithm hashAlgorithm = KeyedHashAlgorithm.Create(_hashAlgorithmName);
            hashAlgorithm.Key = deriveBytes.GetBytes(hashAlgorithm.Key.Length);

            return new SharedSecretDataProtection(symmetricAlgorithm, hashAlgorithm);
        }
    }
}
