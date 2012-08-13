using System.IO;
using Owin;

namespace Katana.Engine
{
    public class StartInfo
    {
        public string Server { get; set; }
        public object ServerFactory { get; set; }

        public IAppBuilder Builder { get; set; }

        public string Startup { get; set; }
        public object App { get; set; }

        public string OutputFile { get; set; }
        public TextWriter Output { get; set; }

        public string Url { get; set; }
        public string Scheme { get; set; }
        public string Host { get; set; }
        public int? Port { get; set; }
        public string Path { get; set; }
    }
}
