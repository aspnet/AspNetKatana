using System;
using Katana.Engine;

namespace Katana.Sample.HelloWorld
{
    class Program
    {
        // Use this project to F5 test different applications and servers together.
        public static void Main(string[] args)
        {
            using (KatanaApplication.Start(
                url: "http://+:8080/",
                app: "Katana.Sample.HelloWorld.Startup",
                server: "Microsoft.HttpListener.Owin"))
            {
                Console.WriteLine("Running, press any key to exit");
                Console.ReadKey();
            }
        }
    }
}
