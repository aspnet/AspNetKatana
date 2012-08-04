using System.IO;
using Owin;
using Gate;
using Gate.Middleware;
using Katana.WebApi;

namespace Katana.Server.AspNet.WebApplication
{
    public class Startup
    {
        public void Configuration(IAppBuilder builder)
        {
            var trace = builder.Properties.Get<TextWriter>("host.TraceOutput");
            trace.WriteLine("Startup taking place");

            builder
                .UseShowExceptions()
                .UseMessageHandler<TraceRequestFilter>()
                .Run(Wilson.App());
        }
    }
}