using System;
using System.Collections.Generic;

namespace Katana.Engine
{
    [Serializable]
    public class StartParameters
    {
        public string Boot { get; set; }

        public string Server { get; set; }

        public string App { get; set; }
        public string OutputFile { get; set; }
        public int Verbosity { get; set; }

        public string Url { get; set; }
        public string Scheme { get; set; }
        public string Host { get; set; }
        public int? Port { get; set; }
        public string Path { get; set; }

        public bool ShowHelp { get; set; }
        public IList<string> HelpArgs { get; set; }
    }
}
