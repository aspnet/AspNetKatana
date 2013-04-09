// <copyright file="DsaSignedDataProtectionProvider.cs" company="Microsoft Open Technologies, Inc.">
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

namespace Microsoft.Owin.Security.DataProtection.SignedData
{
    /// <summary>
    /// Used to produce and verify DSA signatures within protected data.
    /// </summary>
    public class DsaSignedDataProtectionProvider : IDataProtectionProvider
    {
        private readonly DSACryptoServiceProvider _dsa;
        private readonly IDataProtectionProvider _provider;

        /// <summary>
        /// Used to produce and verify DSA signatures within protected data. The signature is appended to the user data, and both
        /// are protected by the given data protection provider.
        /// </summary>
        /// <param name="dsa">The cryptography provider which holds the public key used by Unprotect to verify data signatures. If the
        /// cryptography provider holds the private key then it will be used by Protect to generate the data signature.</param>
        /// <param name="provider">The data protection provider which protects and unprotects the combination of user data and
        /// signature.</param>
        public DsaSignedDataProtectionProvider(DSACryptoServiceProvider dsa, IDataProtectionProvider provider)
        {
            _dsa = dsa;
            _provider = provider;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="purposes"></param>
        /// <returns></returns>
        public IDataProtection Create(params string[] purposes)
        {
            return new DsaSignedDataProtection(_dsa, _provider.Create(purposes));
        }
    }
}
