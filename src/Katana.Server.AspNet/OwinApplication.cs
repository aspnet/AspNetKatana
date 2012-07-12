using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Owin;
using Gate.Builder;

namespace Katana.Server.AspNet
{
    public static class OwinApplication
    {
        static Lazy<AppDelegate> _instance = new Lazy<AppDelegate>(AppBuilder.BuildPipeline<AppDelegate>);

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
