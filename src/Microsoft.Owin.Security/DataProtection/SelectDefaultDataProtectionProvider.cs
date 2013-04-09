using System.Threading;
using Microsoft.Owin.Security.Infrastructure;

namespace Microsoft.Owin.Security.DataProtection
{
    public class SelectDefaultDataProtectionProvider : IDataProtectionProvider
    {
        private IDataProtectionProvider _provider;
        private bool _providerInitialized;
        private object _providerSyncLock;

        public IDataProtection Create(params string[] purposes)
        {
            var provider = LazyInitializer.EnsureInitialized(
                ref _provider,
                ref _providerInitialized,
                ref _providerSyncLock,
                SelectProvider);
            return provider.Create(purposes);
        }

        private IDataProtectionProvider SelectProvider()
        {
            if (HostingEnvironmentApi.IsHosted)
            {
                return new MachineKeyDataProtectionProvider();                
            }
            else
            {
                return new DpapiDataProtectionProvider();
            }
        }
    }
}
