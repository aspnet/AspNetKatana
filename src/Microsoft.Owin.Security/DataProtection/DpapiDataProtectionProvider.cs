namespace Microsoft.Owin.Security.DataProtection
{
    public class DpapiDataProtectionProvider : IDataProtectionProvider
    {
        public IDataProtection Create(params string[] purposes)
        {
            return new DpapiDataProtection(purposes);
        }
    }
}
