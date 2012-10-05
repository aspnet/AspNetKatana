// Copyright 2011-2012 Katana contributors
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using Katana.Engine;

namespace Katana.Sample.HelloWorld
{
    internal class Program
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
                System.Diagnostics.Process.Start("http://localhost:8080/");
                Console.ReadKey();
            }
        }
    }
}
