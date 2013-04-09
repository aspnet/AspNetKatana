// <copyright file="DsaSignedDataProtection.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Security.Cryptography;

namespace Microsoft.Owin.Security.DataProtection.SignedData
{
    internal class DsaSignedDataProtection : IDataProtection
    {
        private readonly DSACryptoServiceProvider _dsa;
        private readonly IDataProtection _dataProtection;

        public DsaSignedDataProtection(DSACryptoServiceProvider dsa, IDataProtection dataProtection)
        {
            _dsa = dsa;
            _dataProtection = dataProtection;
        }

        public byte[] Protect(byte[] userData)
        {
            const int signatureLength = 40;
            byte[] signature = _dsa.SignData(userData);
            var combined = new byte[signatureLength + userData.Length];

            Array.Copy(signature, 0, combined, 0, signatureLength);
            Array.Copy(userData, 0, combined, signatureLength, userData.Length);

            return _dataProtection.Protect(combined);
        }

        public byte[] Unprotect(byte[] protectedData)
        {
            byte[] combined = _dataProtection.Unprotect(protectedData);

            const int signatureLength = 40;
            var signature = new byte[signatureLength];
            var userData = new byte[combined.Length - signature.Length];

            Array.Copy(combined, 0, signature, 0, signature.Length);
            Array.Copy(combined, signature.Length, userData, 0, userData.Length);

            return _dsa.VerifyData(userData, signature) ? userData : null;
        }
    }
}
