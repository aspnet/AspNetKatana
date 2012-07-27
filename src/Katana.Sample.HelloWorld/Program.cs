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
                Server = "HttpListener", // Katana.Server.HttpListener
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

            IDisposable server = engine.Start(info);
            Console.WriteLine("Running, press any key to exit");
            Console.ReadKey();
        }

        public void Configuration(IAppBuilder builder)
        {
            builder.UseShowExceptions().Run(Wilson.App);
        }
    }
}
