using System.Linq;
using System.Security.Cryptography;
using Microsoft.Owin.Security.DataProtection;
using Shouldly;
using Xunit;

namespace Microsoft.Owin.Security.Tests
{
    public class SharedSecretTests
    {
#if NOT_COMPILED
        [Fact]
        public void ProviderCanProtectAndUnprotectData()
        {
            var original = new byte[] { 1, 2, 3, 4, 5 };

            var provider1 = new SharedSecretDataProtectionProvider("This is a test", "AES", "HMACSHA1");
            var protection1 = provider1.Create("Unit", "Test");
            var protectedData = protection1.Protect(original);

            var provider2 = new SharedSecretDataProtectionProvider("This is a test", "AES", "HMACSHA1");
            var protection2 = provider2.Create("Unit", "Test");
            var userData = protection2.Unprotect(protectedData);

            userData.ShouldBe(original);
        }

        [Fact]
        public void AnySizeUpToAThousandWillRoundTrip()
        {
            var provider1 = new SharedSecretDataProtectionProvider("This is a test", "AES", "HMACSHA1");
            var protection1 = provider1.Create("Unit", "Test");

            var provider2 = new SharedSecretDataProtectionProvider("This is a test", "AES", "HMACSHA1");
            var protection2 = provider2.Create("Unit", "Test");

            var rng = new RNGCryptoServiceProvider();
            for (int length = 0; length != 1000; ++length)
            {
                var original = new byte[length];
                rng.GetBytes(original);

                var protectedData = protection1.Protect(original);

                var userData = protection2.Unprotect(protectedData);

                userData.ShouldBe(original);
            }
        }

        [Fact]
        public void ChangingAnyBitWillCauseUnprotectToReturnNull()
        {
            var provider1 = new SharedSecretDataProtectionProvider("This is a test", "AES", "HMACSHA1");
            var protection1 = provider1.Create("Unit", "Test");

            var provider2 = new SharedSecretDataProtectionProvider("This is a test", "AES", "HMACSHA1");
            var protection2 = provider2.Create("Unit", "Test");

            var original = Enumerable.Range(0, 32).Select(x => (byte)x).ToArray();
            var protectedData = protection1.Protect(original);

            for (int index = 0; index != protectedData.Length; ++index)
            {
                for (int bit = 0; bit != 8; ++bit)
                {
                    protectedData[index] ^= (byte)(1 << bit);
                    protection2.Unprotect(protectedData).ShouldBe(null);
                    protectedData[index] ^= (byte)(1 << bit);
                    protection2.Unprotect(protectedData).ShouldBe(original);
                }
            }
        }
#endif
    }
}
