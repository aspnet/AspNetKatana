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
            using (WebApplication.Start<Startup>("http://localhost:7000/"))
            {
                Console.WriteLine("Started");
                Console.ReadKey();
            }
        }
    }
}
