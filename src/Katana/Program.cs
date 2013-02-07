// <copyright file="Program.cs" company="Katana contributors">
//   Copyright 2011-2013 Katana contributors
// </copyright>
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.Hosting.Services;
using NDesk.Options;

namespace Katana
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                return Run(args);
            }
            catch (Exception ex)
            {
                Display(ex);
                return 1;
            }
        }

        public static int Run(string[] args)
        {
            StartOptions options = ParseArguments(args);
            if (options == null)
            {
                return 0;
            }

            WriteLine(options, 1, "Verbose");

            if (options.Boot == null)
            {
                options.Boot = "Domain";
            }

            ResolveAssembliesFromDirectory(
                Path.Combine(Directory.GetCurrentDirectory(), "bin"));

            WriteLine(options, 1, "Starting");

            IServiceProvider services = DefaultServices.Create();
            var starter = services.GetService<IKatanaStarter>();
            IDisposable server = starter.Start(options);

            WriteLine(options, 1, "Started successfully");

            WriteLine(options, 1, "Press Enter to exit");
            Console.ReadLine();

            WriteLine(options, 1, "Terminating.");

            server.Dispose();
            return 0;
        }

        private static void WriteLine(StartOptions options, int verbosity, string message)
        {
            if (verbosity <= options.Verbosity)
            {
                Console.WriteLine(message);
            }
        }

        private static void Display(Exception exception)
        {
            var aggregateException = exception as AggregateException;
            if (aggregateException != null)
            {
                aggregateException.Handle(ex =>
                {
                    Display(ex);
                    return true;
                });
            }
            else if (exception != null)
            {
                Console.WriteLine("Error: {0}{1}  {2}",
                    exception.GetType().FullName,
                    Environment.NewLine,
                    exception.Message);
                Display(exception.InnerException);
            }
        }

        private static void HandleBreak(Action dispose)
        {
            bool cancelPressed = false;
            Console.TreatControlCAsInput = false;
            Console.CancelKeyPress += (_, e) =>
            {
                if (e.SpecialKey == ConsoleSpecialKey.ControlBreak)
                {
                    return;
                }
                if (cancelPressed)
                {
                    dispose();
                    Environment.Exit(-1);
                    e.Cancel = true;
                }
                else
                {
                    cancelPressed = true;
                    Console.WriteLine("Press ctrl+c again to terminate");
                    e.Cancel = true;
                }
            };
        }

        private static StartOptions ParseArguments(IEnumerable<string> args)
        {
            var arguments = new StartOptions();
            OptionSet optionSet = new OptionSet()
                .Add(
                    "s=|server=",
                    @"Load assembly named ""Katana.Server.TYPE.dll"" to determine http server to use. TYPE defaults to HttpListener.",
                    x => arguments.Server = x)
                .Add(
                    "u=|url=",
                    @"May be used to set --scheme, --host, --port, and --path options with a combined URIPREFIX value. Format is '<scheme>://<host>[:<port>]<path>/'.",
                    x => arguments.Url = x)
                .Add(
                    "S=|scheme=",
                    @"Determine which socket protocol server should bind with. SCHEME may be 'http' or 'https'. Defaults to 'http'.",
                    x => arguments.Scheme = x)
                .Add(
                    "h=|host=",
                    @"Which host name or IP address to listen on. NAME defaults to '+' for all IP addresses.",
                    x => arguments.Host = x)
                .Add(
                    "p=|port=",
                    @"Which TCP port to listen on. NUMBER defaults to 8080.",
                    (int x) => arguments.Port = x)
                .Add(
                    "P=|path=",
                    @"Determines the virtual directory to run use as the base path for <application> requests. PATH must start with a '/'.",
                    x => arguments.Path = x)
                .Add(
                    "o=|output=",
                    @"Writes any errors and trace logging to FILE. Default is stderr.",
                    x => arguments.OutputFile = x)
                .Add(
                    "v|verbose",
                    @"Increase the output verbosity.",
                    x =>
                    {
                        if (x != null)
                        {
                            ++arguments.Verbosity;
                        }
                    })
                .Add(
                    "?|help",
                    @"Show this message and exit.",
                    x => arguments.ShowHelp = x != null)
                .Add(
                    "b=|boot=",
                    @"Loads assembly named ""Katana.Boot.VALUE.dll"" to provide custom startup control.",
                    x => arguments.Boot = x);

            List<string> extra;
            try
            {
                extra = optionSet.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("Katana: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try 'Katana --help' for more information.");
                return null;
            }
            if (arguments.ShowHelp)
            {
                ShowHelp(optionSet, extra);
                return null;
            }
            arguments.App = string.Join(" ", extra.ToArray());
            return arguments;
        }

        private static void ShowHelp(OptionSet optionSet, IEnumerable<string> helpArgs)
        {
            Console.Write(
                @"Usage: Katana [options] [<application>]
Runs <application> on an http server
Example: Katana -p8080 HelloWorld.Startup

Options:
");
            optionSet.WriteOptionDescriptions(Console.Out);
            Console.Write(
                @"
Environment Variables:
PORT                         Changes the default TCP port to listen on when 
                               both --port and --url options are not provided.
OWIN_SERVER                  Changes the default server TYPE to use when
                               the --server option is not provided.

");
        }

        [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Reflection.Assembly.LoadFile", Justification = "By design")]
        public static void ResolveAssembliesFromDirectory(string directory)
        {
            var cache = new Dictionary<string, Assembly>();
            AppDomain.CurrentDomain.AssemblyResolve +=
                (a, b) =>
                {
                    Assembly assembly;
                    if (cache.TryGetValue(b.Name, out assembly))
                    {
                        return assembly;
                    }

                    string shortName = new AssemblyName(b.Name).Name;
                    string path = Path.Combine(directory, shortName + ".dll");
                    if (File.Exists(path))
                    {
                        assembly = Assembly.LoadFile(path);
                    }
                    cache[b.Name] = assembly;
                    if (assembly != null)
                    {
                        cache[assembly.FullName] = assembly;
                    }
                    return assembly;
                };
        }
    }
}
