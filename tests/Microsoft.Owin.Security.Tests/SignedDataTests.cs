// <copyright file="SignedDataTests.cs" company="Microsoft Open Technologies, Inc.">
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
using Microsoft.Owin.Security.DataProtection;
using System.Security.Cryptography;
using Shouldly;
using Xunit;

namespace Microsoft.Owin.Security.Tests
{
    public class SignedDataTests
    {
#if NOT_COMPILED
        [Fact]
        public void RsaWillSignAndVerifyData()
        {
            var provider = new RsaSignedDataProtectionProvider(new RSACryptoServiceProvider(), new DpapiDataProtectionProvider());
            var protection = provider.Create("one", "two", "three");
            var original = new byte[] { 1, 2, 3, 4, 5 };
            var protectedData = protection.Protect(original);
            var userData = protection.Unprotect(protectedData);

            userData.ShouldBe(original);
        }

        [Fact]
        public void RsaPublicKeyWillVerifyPrivateKeySignature()
        {
            var publicKey = new RSACryptoServiceProvider();
            var privateKey = new RSACryptoServiceProvider();

            publicKey.ImportParameters(privateKey.ExportParameters(includePrivateParameters: false));

            var publicProvider = new RsaSignedDataProtectionProvider(publicKey, new DpapiDataProtectionProvider());
            var privateProvider = new RsaSignedDataProtectionProvider(privateKey, new DpapiDataProtectionProvider());

            var publicProtection = publicProvider.Create("alpha", "beta");
            var privateProtection = privateProvider.Create("alpha", "beta");

            var original = new byte[] { 1, 2, 3, 4, 5 };
            var protectedData = privateProtection.Protect(original);
            var userData = publicProtection.Unprotect(protectedData);

            userData.ShouldBe(original);
        }

        [Fact]
        public void DsaWillSignAndVerifyData()
        {
            var provider = new DsaSignedDataProtectionProvider(new DSACryptoServiceProvider(), new DpapiDataProtectionProvider());
            var protection = provider.Create("one", "two", "three");
            var original = new byte[] { 1, 2, 3, 4, 5 };
            var protectedData = protection.Protect(original);
            var userData = protection.Unprotect(protectedData);

            userData.ShouldBe(original);
        }

        [Fact]
        public void DsaPublicKeyWillVerifyPrivateKeySignature()
        {
            var publicKey = new DSACryptoServiceProvider();
            var privateKey = new DSACryptoServiceProvider();

            publicKey.ImportParameters(privateKey.ExportParameters(includePrivateParameters: false));

            var publicProvider = new DsaSignedDataProtectionProvider(publicKey, new DpapiDataProtectionProvider());
            var privateProvider = new DsaSignedDataProtectionProvider(privateKey, new DpapiDataProtectionProvider());

            var publicProtection = publicProvider.Create("alpha", "beta");
            var privateProtection = privateProvider.Create("alpha", "beta");

            var original = new byte[] { 1, 2, 3, 4, 5 };
            var protectedData = privateProtection.Protect(original);
            var userData = publicProtection.Unprotect(protectedData);

            userData.ShouldBe(original);
        }
#endif
    }
}
