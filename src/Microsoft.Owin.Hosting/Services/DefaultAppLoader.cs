using System;
using Owin;
using Owin.Loader;

namespace Microsoft.Owin.Hosting.Services
{
    public class DefaultAppLoader : IAppLoader
    {
        public static IAppLoader CreateInstance()
        {
            return new DefaultAppLoader();
        }

        public Action<IAppBuilder> Load(string appName)
        {
            return new DefaultLoader().Load(appName);
        }
    }
}