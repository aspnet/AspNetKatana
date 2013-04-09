namespace Microsoft.Owin.Security.DataProtection
{
    public interface IDataProtectionProvider
    {
        IDataProtection Create(params string[] purposes);
    }
}
