using System.IO;
using Gate.Builder.Loader;
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

        IStartupLoader Loader { get; }
        IAppBuilder Builder { get; }
    }
}
