using System.Web.Security;

namespace Microsoft.Owin.Security.Infrastructure
{
    internal static class MachineKeyApi
    {
        private static readonly IApi Call = new Api();

        public static byte[] Protect(byte[] userData, string[] purposes)
        {
            return Call.Protect(userData, purposes);
        }

        public static byte[] Unprotect(byte[] protectedData, string[] purposes)
        {
            return Call.Unprotect(protectedData, purposes);
        }

        public interface IApi
        {
            byte[] Protect(byte[] userData, string[] purposes);
            byte[] Unprotect(byte[] protectedData, string[] purposes);
        }

        public class Api : IApi
        {
            public byte[] Protect(byte[] userData, string[] purposes)
            {
                return MachineKey.Protect(userData, purposes);
            }

            public byte[] Unprotect(byte[] protectedData, string[] purposes)
            {
                return MachineKey.Unprotect(protectedData, purposes);
            }
        }
    }
}
