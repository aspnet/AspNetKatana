using System;
using Owin;

namespace Microsoft.Owin.Hosting.Loader
{
    public interface IAppLoaderProvider
    {
        Func<string, Action<IAppBuilder>> GetAppLoader();
    }
}
