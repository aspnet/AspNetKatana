using System;
using System.Configuration;
using System.Web;
using Gate.Builder;
using Gate.Builder.Loader;
using Katana.Server.AspNet.CallEnvironment;
using Owin;

namespace Katana.Server.AspNet
{
    static class OwinBuilder
    {
        public static AppDelegate Build()
        {
            var configuration = ConfigurationManager.AppSettings["owin:Configuration"];
            var startup = new StartupLoader().Load(configuration);
            return Build(startup);
        }

        public static AppDelegate Build(Action<IAppBuilder> startup)
        {
            return AppBuilder.BuildPipeline<AppDelegate>(builder =>
            {
                builder.Properties["host.TraceOutput"] = TraceTextWriter.Instance;
                startup(builder);
            });
        }
    }
}
