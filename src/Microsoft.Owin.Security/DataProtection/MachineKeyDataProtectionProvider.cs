namespace Microsoft.Owin.Security.DataProtection
{
    public class MachineKeyDataProtectionProvider : IDataProtectionProvider
    {
        public IDataProtection Create(params string[] purposes)
        {
            return new MachineKeyDataProtection(purposes);
        }
    }
}
