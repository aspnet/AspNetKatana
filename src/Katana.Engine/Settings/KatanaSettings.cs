using System;
using System.IO;
using Gate.Builder;
using Gate.Builder.Loader;
using Owin;

namespace Katana.Engine.Settings
{
    public class KatanaSettings : IKatanaSettings
    {
        public KatanaSettings()
        {
            DefaultServer = "HttpListener";

            DefaultScheme = "http";
            DefaultHost = "+";
            DefaultPort = 8080;

            DefaultOutput = Console.Error;

            ServerAssemblyPrefix = "Katana.Server.";
            
            Loader = new StartupLoader();
            Builder = new AppBuilder();
        }

        public string DefaultServer { get; set; }

        public string DefaultScheme { get; set; }
        public string DefaultHost { get; set; }
        public int? DefaultPort { get; set; }

        public TextWriter DefaultOutput { get; set; }

        public string ServerAssemblyPrefix { get; set; }

        public IStartupLoader Loader { get; set; }
        public IAppBuilder Builder { get; set; }
    }
}