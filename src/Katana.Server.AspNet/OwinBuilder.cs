using System;
using System.Configuration;
using Katana.Server.AspNet.CallEnvironment;
using Owin;
using Owin.Builder;
using Owin.Loader;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

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
            DetectWebSocketSupport(builder);
            startup(builder);
            return builder.Build<AppDelegate>();
        }

        private static void DetectWebSocketSupport(IAppBuilder builder)
        {
            // There is no explicit API to detect server side websockets, just check for v4.5 / Win8.
            // Per request we can provide actual verification.
            if (Environment.OSVersion.Version >= new Version(6, 2))
            {
                try
                {
                    Assembly webSocketMiddlewareAssembly = Assembly.Load("Katana.Server.DotNetWebSockets");

                    webSocketMiddlewareAssembly.GetType("Katana.Server.DotNetWebSockets.WebSocketWrapperExtensions")
                        .GetMethod("UseAspNetWebSocketWrapper")
                        .Invoke(null, new object[] { builder });

                    builder.Properties[Constants.WebSocketSupportKey] = Constants.WebSocketSupport;
                }
                catch (Exception)
                {
                    // TODO: Trace
                }
            }
            else
            {
                // TODO: Trace
            }
        }
    }
}
