namespace Microsoft.Owin.Security.DataProtection
{
    public interface IDataProtection
    {
        byte[] Protect(byte[] userData);
        byte[] Unprotect(byte[] protectedData);
    }
}
