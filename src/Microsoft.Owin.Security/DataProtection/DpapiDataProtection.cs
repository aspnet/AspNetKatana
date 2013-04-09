using System.Security.Cryptography;

namespace Microsoft.Owin.Security.DataProtection
{
    public class DpapiDataProtection : IDataProtection
    {
        private readonly DpapiDataProtector _protector;

        public DpapiDataProtection(string[] purposes)
        {
            _protector = new DpapiDataProtector("Microsoft.Owin.Security", "IDataProtection", purposes)
            {
                Scope = DataProtectionScope.CurrentUser
            };
        }

        public byte[] Protect(byte[] userData)
        {
            return _protector.Protect(userData);
        }

        public byte[] Unprotect(byte[] protectedData)
        {
            return _protector.Unprotect(protectedData);
        }
    }
}
