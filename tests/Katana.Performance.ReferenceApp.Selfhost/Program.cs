// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;

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
