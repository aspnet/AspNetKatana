using System;
using System.IO;
using Owin;

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

        ILoader Loader { get; }
        Func<IAppBuilder> BuilderFactory { get; }
    }
}
