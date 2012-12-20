using System;
using Microsoft.Owin.Hosting.Builder;
using Owin;
using Owin.Loader;

namespace Microsoft.Owin.Hosting.Loader
{
    public class DefaultAppLoaderProvider : IAppLoaderProvider
    {
        private readonly IAppActivator _activator;

        public DefaultAppLoaderProvider(IAppActivator activator)
        {
            _activator = activator;
        }

        public Func<string, Action<IAppBuilder>> GetAppLoader()
        {
            var loader = new DefaultLoader(_activator.Activate);
            return loader.Load;
        }
    }
}