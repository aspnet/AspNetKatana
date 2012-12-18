using System;
using System.Collections.Generic;
using System.Linq;
using Owin;

namespace Microsoft.Owin.Hosting.Services
{
    public interface IAppLoaderChain
    {
        Action<IAppBuilder> Load(string appName);
    }

    public class DefaultAppLoaderChain : IAppLoaderChain
    {
        private readonly IEnumerable<IAppLoader> _loaders;

        public DefaultAppLoaderChain(IEnumerable<IAppLoader> loaders)
        {
            if (loaders == null) throw new ArgumentNullException("loaders");

            _loaders = loaders;
        }

        public static IAppLoaderChain CreateInstance(IServiceProvider services)
        {
            return new DefaultAppLoaderChain(
                services.GetService<IEnumerable<IAppLoader>>());
        }

        public Action<IAppBuilder> Load(string appName)
        {
            return _loaders.Aggregate(
                default(Action<IAppBuilder>),
                (app, loader) => app ?? loader.Load(appName));
        }
    }
}
