using System.Security.Cryptography;

namespace Microsoft.Owin.Security.DataProtection
{
    public class DsaSignedDataProtectionProvider : IDataProtectionProvider
    {
        private readonly DSACryptoServiceProvider _dsa;
        private readonly IDataProtectionProvider _provider;

        public DsaSignedDataProtectionProvider(DSACryptoServiceProvider dsa, IDataProtectionProvider provider)
        {
            _dsa = dsa;
            _provider = provider;
        }

        public IDataProtection Create(params string[] purposes)
        {
            return new DsaSignedDataProtection(_dsa, _provider.Create(purposes));
        }
    }
}
