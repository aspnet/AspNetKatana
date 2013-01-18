// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
// </copyright>
// -----------------------------------------------------------------------

using System;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.Hosting.Services;
using Microsoft.Owin.StaticFiles.FileSystems;

namespace Katana.Performance.ReferenceApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var services = DefaultServices.Create(cfg =>
                cfg.AddInstance<IFileSystemProvider>(new PhysicalFileSystemProvider("Public"))            
            );

            var starter = services.GetService<IKatanaStarter>();

            using (starter.Start<Startup>("http://localhost:12345/"))
            {
                Console.WriteLine("Started");
                Console.ReadKey();
            }
        }
    }
}
