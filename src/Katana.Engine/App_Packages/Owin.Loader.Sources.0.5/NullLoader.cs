using System;

namespace Owin.Loader
{
    internal class NullLoader
    {
        static readonly NullLoader _singleton = new NullLoader();

        public static Func<string, Action<IAppBuilder>> Instance { get { return _singleton.Load; } }

        public Action<IAppBuilder> Load(string startup)
        {
            return null;
        }
    }
}
