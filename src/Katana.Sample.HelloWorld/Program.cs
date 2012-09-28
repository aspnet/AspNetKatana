using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Katana.Sample.HelloWorld
{
    using System;
    using Katana.Engine;
    using Katana.Engine.Settings;
    using Owin;
    using Gate.Middleware;
    using Gate;

    class Program
    {
        // Use this project to F5 test different applications and servers together.
        public static void Main(string[] args)
        {
            var settings = new KatanaSettings();

            KatanaEngine engine = new KatanaEngine(settings);

            var info = new StartInfo
            {
                Server = "Microsoft.HttpListener.Owin", // Katana.Server.HttpListener
                Startup = "Katana.Sample.HelloWorld.Program.Configuration", // Application
                Url = "http://+:8080/",
                /*
                OutputFile = string.Empty,
                Scheme = arguments.Scheme,
                Host = arguments.Host,
                Port = arguments.Port,
                Path = arguments.Path,
                 */
            };

            using (engine.Start(info))
            {
                Console.WriteLine("Running, press any key to exit");
                Console.ReadKey();
            }
        }

        public void Configuration(IAppBuilder builder)
        {
            var traceOutput = builder.Properties.Get<TextWriter>("host.TraceOutput");
            var addresses = builder.Properties.Get<IList<IDictionary<string, object>>>("host.Addresses");
            var onAppDisposing = builder.Properties.Get<Action<Action>>("host.OnAppDisposing");

            traceOutput.WriteLine("Starting");

            addresses.Add(new Dictionary<string, object>
            {
                {"scheme", "http"},
                {"host", "+"},
                {"port", "8081"},
                {"path", "/hello"},
            });

            builder
                .UseShowExceptions()
                .Run(Wilson.App());

            onAppDisposing(
                () =>
                {
                    traceOutput.WriteLine("Stopping"); 
                    traceOutput.Flush();
                    Thread.Sleep(TimeSpan.FromSeconds(2.5));
                });
        }
    }
}
