//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Katana Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;

namespace Katana.Engine
{
    public static class KatanaApplication
    {
        public static IDisposable Start(
            string app = null,
            string url = null,
            string server = null,
            string scheme = null,
            string host = null,
            int? port = null,
            string path = null,
            string boot = null,
            string outputFile = null,
            int verbosity = 0)
        {
            return Start(
                new StartParameters
                {
                    Boot = boot,

                    Server = server,

                    App = app,
                    OutputFile = outputFile,
                    Verbosity = verbosity,

                    Url = url,
                    Scheme = scheme,
                    Host = host,
                    Port = port,
                    Path = path,
                });
        }

        public static IDisposable Start(StartParameters parameters)
        {
            return new KatanaStarter().Start(parameters);
        }
    }
}
