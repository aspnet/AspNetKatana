// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
// </copyright>
// -----------------------------------------------------------------------

using System;
using Microsoft.Owin.Hosting;

namespace Katana.Performance.ReferenceApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using (WebApplication.Start<Startup>("http://localhost:12345/"))
            {
                Console.WriteLine("Started");
                Console.ReadKey();
            }
        }
    }
}
