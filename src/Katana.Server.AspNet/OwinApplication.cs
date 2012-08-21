using System;
using Owin;

namespace Microsoft.AspNet.Owin
{
    public static class OwinApplication
    {
        static Lazy<AppDelegate> _instance = new Lazy<AppDelegate>(OwinBuilder.Build);

        public static AppDelegate Instance
        {
            get { return _instance.Value; }
            set { _instance = new Lazy<AppDelegate>(() => value); }
        }

        public static Func<AppDelegate> Accessor
        {
            get { return () => _instance.Value; }
            set { _instance = new Lazy<AppDelegate>(value); }
        }
    }
}
