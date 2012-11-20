using Katana.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Katana.Performance.ReferenceApp
{
    class Program
    {
        static void Main(string[] args)
        {
            new KatanaStarter().Start(new StartParameters() { App = "Katana.Performance.ReferenceApp.Startup" });
            Console.WriteLine("Started");
            Console.ReadKey();
        }
    }
}
