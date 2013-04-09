using Microsoft.Owin.Security.Infrastructure;

namespace Microsoft.Owin.Security.DataProtection
{
    public class MachineKeyDataProtection : IDataProtection
    {
        private readonly string[] _purposes;

        public MachineKeyDataProtection(params string[] purposes)
        {
            _purposes = purposes;
        }

        public byte[] Protect(byte[] userData)
        {
            return MachineKeyApi.Protect(userData, _purposes);
        }

        public byte[] Unprotect(byte[] protectedData)
        {
            return MachineKeyApi.Unprotect(protectedData, _purposes);
        }
    }
}
