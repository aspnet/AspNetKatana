using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Katana.Engine;

namespace Katana.Performance.ReferenceApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            new KatanaStarter().Start(new StartParameters() { App = "Katana.Performance.ReferenceApp.Startup" });
            Console.WriteLine("Started");
            Console.ReadKey();
        }
    }
}
