using System;
using System.IO;
using Owin;
using Owin.Loader;

namespace Katana.Engine.Settings
{
    public interface IKatanaSettings
    {
        string DefaultServer { get; }
        string DefaultScheme { get; }
        string DefaultHost { get; }
        int? DefaultPort { get; }
        TextWriter DefaultOutput { get; }

        string ServerAssemblyPrefix { get; }

        Func<IStartupLoader> LoaderFactory { get; }
        Func<IAppBuilder> BuilderFactory { get; }
    }
}
