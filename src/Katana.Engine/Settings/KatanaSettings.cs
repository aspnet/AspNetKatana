using System;
using System.IO;
using Owin;
using Owin.Builder;
using Owin.Loader;

namespace Katana.Engine.Settings
{
    public class KatanaSettings : IKatanaSettings
    {
        Func<IStartupLoader> _loaderFactory;

        public KatanaSettings()
        {
            DefaultServer = "HttpListener";

            DefaultScheme = "http";
            DefaultHost = "+";
            DefaultPort = 8080;

            DefaultOutput = Console.Error;

            ServerAssemblyPrefix = "Katana.Server.";

            LoaderFactory = () => new DefaultLoader();
            BuilderFactory = () => new AppBuilder();
        }

        public string DefaultServer { get; set; }

        public string DefaultScheme { get; set; }
        public string DefaultHost { get; set; }
        public int? DefaultPort { get; set; }

        public TextWriter DefaultOutput { get; set; }

        public string ServerAssemblyPrefix { get; set; }

        public Func<IStartupLoader> LoaderFactory { get; set; }
        public Func<IAppBuilder> BuilderFactory { get; set; }
    }
}