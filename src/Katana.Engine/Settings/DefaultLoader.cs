using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gate.Builder.Loader;
using Owin;

namespace Katana.Engine.Settings
{
    public class DefaultLoader : ILoader
    {
        IStartupLoader _loader;

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
