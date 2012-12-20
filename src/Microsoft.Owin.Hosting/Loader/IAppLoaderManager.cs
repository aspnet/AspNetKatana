using System;
using Owin;

namespace Microsoft.Owin.Hosting.Loader
{
    public interface IAppLoaderManager
    {
        Action<IAppBuilder> Load(string appName);
    }
}
