using System.Security.Cryptography;

namespace Microsoft.Owin.Security.DataProtection
{
    public class RsaSignedDataProtectionProvider : IDataProtectionProvider
    {
        private readonly RSACryptoServiceProvider _rsa;
        private readonly IDataProtectionProvider _provider;

        public RsaSignedDataProtectionProvider(RSACryptoServiceProvider rsa, IDataProtectionProvider provider)
        {
            _rsa = rsa;
            _provider = provider;
        }

        public IDataProtection Create(params string[] purposes)
        {
            return new RsaSignedDataProtection(_rsa, _provider.Create(purposes));
        }
    }
}
