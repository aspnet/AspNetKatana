using System;
using System.Collections.Generic;
using System.Linq;
using Owin;

namespace Microsoft.Owin.Hosting.Loader
{
    public class DefaultAppLoaderManager : IAppLoaderManager
    {
        private readonly IEnumerable<IAppLoaderProvider> _providers;

        public DefaultAppLoaderManager(IEnumerable<IAppLoaderProvider> providers)
        {
            if (providers == null) throw new ArgumentNullException("providers");

            _providers = providers;
        }

        public Action<IAppBuilder> Load(string appName)
        {
            return _providers.Aggregate(
                default(Action<IAppBuilder>),
                (app, loader) => app ?? loader.GetAppLoader().Invoke(appName));
        }
    }
}