using System;

namespace Katana.Engine
{
    public static class KatanaApplication
    {
        public static IDisposable Start(StartParameters parameters)
        {
            return new KatanaStarter().Start(parameters);
        }
    }
}
