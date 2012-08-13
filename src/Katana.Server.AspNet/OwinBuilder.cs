using System;
using System.Configuration;
using Katana.Server.AspNet.CallEnvironment;
using Owin;
using Owin.Builder;
using Owin.Loader;

namespace Katana.Server.AspNet
{
    static class OwinBuilder
    {
        public static AppDelegate Build()
        {
            var configuration = ConfigurationManager.AppSettings["owin:Configuration"];
            var loader = new DefaultLoader();
            var startup = loader.Load(configuration);
            return Build(startup);
        }

        public static AppDelegate Build(Action<IAppBuilder> startup)
        {
            var builder = new AppBuilder();
            builder.Properties["host.TraceOutput"] = TraceTextWriter.Instance;
            startup(builder);
            return builder.Build<AppDelegate>();
        }
    }
}
