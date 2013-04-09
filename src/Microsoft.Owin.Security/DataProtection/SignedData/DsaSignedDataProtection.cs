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
        private const int SignatureLength = 40;
        private readonly DSACryptoServiceProvider _dsa;
        private readonly IDataProtection _dataProtection;

        public DsaSignedDataProtection(DSACryptoServiceProvider dsa, IDataProtection dataProtection)
        {
            _dsa = dsa;
            _dataProtection = dataProtection;
        }

        public byte[] Protect(byte[] userData)
        {
            byte[] signature = _dsa.SignData(userData);
            var combined = new byte[SignatureLength + userData.Length];

            Array.Copy(signature, 0, combined, 0, SignatureLength);
            Array.Copy(userData, 0, combined, SignatureLength, userData.Length);

            return _dataProtection.Protect(combined);
        }

        public byte[] Unprotect(byte[] protectedData)
        {
            byte[] combined = _dataProtection.Unprotect(protectedData);

            var signature = new byte[SignatureLength];
            var userData = new byte[combined.Length - signature.Length];

            Array.Copy(combined, 0, signature, 0, signature.Length);
            Array.Copy(combined, signature.Length, userData, 0, userData.Length);

            return _dsa.VerifyData(userData, signature) ? userData : null;
        }
    }
}
