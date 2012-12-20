using System;
using Owin;
using Owin.Loader;

namespace Microsoft.Owin.Hosting.Services
{
    public class DefaultAppLoader : IAppLoader
    {
        private readonly IAppActivator _activator;

        public DefaultAppLoader(IAppActivator activator)
        {
            _activator = activator;
        }

        public Action<IAppBuilder> Load(string appName)
        {
            return new DefaultLoader(_activator.Activate).Load(appName);
        }
    }
}