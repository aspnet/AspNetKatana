// <copyright file="RsaSignedDataProtectionProvider.cs" company="Microsoft Open Technologies, Inc.">
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
using Microsoft.Owin.Security.DataProtection.SignedData;

namespace Microsoft.Owin.Security.DataProtection
{
    /// <summary>
    /// Used to produce and verify RSA signatures within protected data.
    /// </summary>
    public class RsaSignedDataProtectionProvider : IDataProtectionProvider
    {
        private readonly RSACryptoServiceProvider _rsa;
        private readonly IDataProtectionProvider _provider;

        /// <summary>
        /// Used to produce and verify RSA signatures within protected data. The signature is appended to the user data, and both
        /// are protected by the given data protection provider.
        /// </summary>
        /// <param name="rsa">The cryptography provider which holds the public key used by Unprotect to verify data signatures. If the
        /// cryptography provider holds the private key then it will be used by Protect to generate the data signature.</param>
        /// <param name="provider">The data protection provider which protects and unprotects the combination of user data and
        /// signature.</param>
        public RsaSignedDataProtectionProvider(RSACryptoServiceProvider rsa, IDataProtectionProvider provider)
        {
            _rsa = rsa;
            _provider = provider;
        }

        /// <summary>
        /// Returns a new instance of IDataProtection for the provider.
        /// </summary>
        /// <param name="purposes">Additional entropy used to ensure protected data may only be unprotected for the correct purposes.</param>
        /// <returns>An instance of a data protection service</returns>
        public IDataProtection Create(params string[] purposes)
        {
            return new RsaSignedDataProtection(_rsa, _provider.Create(purposes));
        }
    }
}
