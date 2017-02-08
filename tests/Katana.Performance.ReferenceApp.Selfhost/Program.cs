// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Owin.Hosting;

namespace Katana.Performance.ReferenceApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using (WebApp.Start<Startup>("http://localhost:12345/"))
            {
                Console.WriteLine("Started");
                Console.ReadKey();
            }
        }
    }
}
