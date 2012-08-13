using System;
using Gate.Builder.Loader;
using Owin;

namespace Katana.Engine.Settings
{
    public class DefaultLoader : ILoader
    {
        readonly IStartupLoader _loader;

        public DefaultLoader()
        {
            _loader = new StartupLoader();
        }

        public Action<IAppBuilder> Load(string startupName)
        {
            return _loader.Load(startupName);
        }
    }
}
