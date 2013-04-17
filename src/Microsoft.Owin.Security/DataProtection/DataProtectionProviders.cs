namespace Microsoft.Owin.Security.DataProtection
{
    public static class DataProtectionProviders
    {
        private static IDataProtectionProvider _default = new SelectDefaultDataProtectionProvider();

        public static IDataProtectionProvider Default
        {
            get { return _default; }
            set { _default = value; }
        }
    }
}
