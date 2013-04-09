using System.Security.Cryptography;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.DataProtection.SignedData;
using Shouldly;
using Xunit;

namespace Microsoft.Owin.Security.Tests
{
    public class SignedDataTests
    {
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
    }
}
