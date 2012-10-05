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
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using NDesk.Options;

namespace Katana.Engine.CommandLine
{
    public class Program
    {
        // P/Invoke:
        private enum FileType
        {
            Unknown,
            Disk,
            Char,
            Pipe
        }

        private enum StdHandle
        {
            Stdin = -10,
            Stdout = -11,
            Stderr = -12
        }

        public static bool IsInputRedirected
        {
            get { return FileType.Char != GetFileType(GetStdHandle(StdHandle.Stdin)); }
        }

        public static void Main(string[] args)
        {
            var parameters = ParseArguments(args);
            if (parameters == null)
            {
                return;
            }

            if (parameters.Boot == null)
            {
                parameters.Boot = "Default";
            }

            DomainManager.ResolveAssembliesFromDirectory(
                Path.Combine(Directory.GetCurrentDirectory(), "bin"));

            var starter = new KatanaStarter();
            var server = starter.Start(parameters);

            if (IsInputRedirected)
            {
                // read a single line that will never arrive, I guess...
                // what's the best way to signal userless console process to exit?

                Console.ReadLine();
            }
            else
            {
                HandleBreak(server.Dispose);

                while (true)
                {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Escape)
                    {
                        break;
                    }
                }
            }

            try
            {
                server.Dispose();
            }
            catch
            {
            }
        }

        [DllImport("kernel32.dll")]
        private static extern FileType GetFileType(IntPtr hdl);
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetStdHandle(StdHandle std);

        private static void HandleBreak(Action dispose)
        {
            var cancelPressed = false;
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

        private static StartParameters ParseArguments(IEnumerable<string> args)
        {
            var arguments = new StartParameters();
            var optionSet = new OptionSet()
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
    }
}
