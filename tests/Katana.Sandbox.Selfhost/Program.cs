
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.Host.HttpListener;
using Owin;

namespace Katana.Sandbox.Selfhost
{
    class Program
    {
        static void Main()
        {
            var address = "http://localhost:8000/";
            using (var server = WebApp.Start(address, appBuilder =>
            {
                var owinHttpListener = appBuilder.Properties[typeof(OwinHttpListener).FullName] as OwinHttpListener;
                appBuilder.Use(async (context, next) => await context.Response.WriteAsync("Hello world!"));
            }))
            {
                Console.WriteLine("Listening on " + address);
                Console.ReadKey();
            }
        }
    }
}
