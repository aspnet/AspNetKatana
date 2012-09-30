using System.IO;
using Owin;

namespace Katana.Engine
{
    public class StartContext
    {
        public StartContext() { Parameters = new StartParameters(); }

        public StartParameters Parameters { get; set; }

        public object ServerFactory { get; set; }
        public IAppBuilder Builder { get; set; }
        public object App { get; set; }
        public TextWriter Output { get; set; }
    }
}
