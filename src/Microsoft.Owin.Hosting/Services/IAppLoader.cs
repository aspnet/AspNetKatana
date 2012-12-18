using System;
using Owin;

namespace Microsoft.Owin.Hosting.Services
{
    public interface IAppLoader
    {
        Action<IAppBuilder> Load(string appName);
    }
}
