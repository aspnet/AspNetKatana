using System.IO;
using Owin;
using Gate.Middleware;

namespace Microsoft.AspNet.Owin.WebApplication
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