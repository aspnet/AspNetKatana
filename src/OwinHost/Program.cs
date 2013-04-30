// <copyright file="Program.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
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
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.Hosting.Services;
using Microsoft.Owin.Hosting.Starter;
using Microsoft.Owin.Hosting.Utilities;
using OwinHost.CommandLine;

namespace OwinHost
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

            bool traceVerbose = IsVerboseTraceEnabled(options);
            WriteLine(options, traceVerbose, "Verbose");

            string boot;
            if (!options.Settings.TryGetValue("boot", out boot)
                || string.IsNullOrWhiteSpace(boot))
            {
                options.Settings["boot"] = "Domain";
            }

            // Have the auto-discovery algorithms include the location of this exe so we can detect server assemblies.
            string privateBin;
            if (!options.Settings.TryGetValue("privatebin", out privateBin)
                || string.IsNullOrWhiteSpace(privateBin))
            {
                options.Settings["privatebin"] = Path.GetDirectoryName(typeof(Program).Assembly.Location);
            }

            ResolveAssembliesFromDirectory(
                Path.Combine(Directory.GetCurrentDirectory(), "bin"));

            WriteLine(options, traceVerbose, "Starting");

            IServiceProvider services = ServicesFactory.Create();
            var starter = services.GetService<IHostingStarter>();
            IDisposable server = starter.Start(options);

            WriteLine(options, traceVerbose, "Started successfully");

            WriteLine(options, traceVerbose, "Press Enter to exit");
            Console.ReadLine();

            WriteLine(options, traceVerbose, "Terminating.");

            server.Dispose();
            return 0;
        }

        private static bool IsVerboseTraceEnabled(StartOptions options)
        {
            string verbose;
            if (options.Settings.TryGetValue("traceverbosity", out verbose)
                && !string.IsNullOrWhiteSpace(verbose))
            {
                int level;
                if (Int32.TryParse(verbose, NumberStyles.None, CultureInfo.InvariantCulture, out level))
                {
                    return level > 0;
                }
            }
            return false;
        }

        private static void WriteLine(StartOptions options, bool verbose, string message)
        {
            if (verbose)
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
            var options = new StartOptions();
            bool showHelp = false;

            CommandLineParser parser = new CommandLineParser();
            parser.Options.Add(new CommandLineOption(
                    new[] { "s", "serverfactory" },
                    @"Load the specified server factory type or assembly. The default is to auto-detect.",
                    x => options.ServerFactory = x));
            parser.Options.Add(new CommandLineOption(
                    new[] { "u", "url" },
                    @"Format is '<scheme>://<host>[:<port>]<path>/'.",
                    x => options.Urls.Add(x)));
            parser.Options.Add(new CommandLineOption(
                    new[] { "p", "port" },
                    @"Which TCP port to listen on. Default is 5000.",
                    x => options.Port = int.Parse(x, CultureInfo.InvariantCulture)));
            parser.Options.Add(new CommandLineOption(
                    new[] { "d", "directory" },
                    @"Specifies the target directory of the application.",
                    x => options.Settings["directory"] = x));
            parser.Options.Add(new CommandLineOption(
                    new[] { "t", "traceoutput" },
                    @"Writes any trace data to the given FILE. Default is stderr.",
                    x => options.Settings["traceoutput"] = x));
            parser.Options.Add(new CommandLineOption(
                    new[] { "settings" },
                    @"The settings file that contains service and setting overrides. Otherwise they are read from the AppSettings section of the app's config file.",
                    x => LoadSettings(options, x)));
            parser.Options.Add(new CommandLineOption(
                    new[] { "v", "traceverbosity" },
                    @"Enable verbose tracing.",
                    x =>
                    {
                        if (string.IsNullOrWhiteSpace(x))
                        {
                            x = "1";
                        }
                        options.Settings["traceverbosity"] = x;
                    }));
            parser.Options.Add(new CommandLineOption(
                    new[] { "b", "boot" },
                    @"Loads an assembly to provide custom startup control.",
                    x => options.Settings["boot"] = x));
            parser.Options.Add(new CommandLineOption(
                    new[] { "?", "help" },
                    @"Show this message and exit.",
                    x => showHelp = true));

            IList<string> extra;
            try
            {
                extra = parser.Parse(args);
            }
            catch (FormatException e)
            {
                Console.Write("OwinHost: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try 'OwinHost /?' for more information.");
                return null;
            }
            if (showHelp)
            {
                ShowHelp(parser);
                return null;
            }
            options.AppStartup = string.Join(" ", extra.ToArray());
            return options;
        }

        private static void LoadSettings(StartOptions options, string settingsFile)
        {
            SettingsLoader.LoadFromSettingsFile(settingsFile, options.Settings);
        }

        private static void ShowHelp(CommandLineParser parser)
        {
            Console.WriteLine("Usage: OwinHost [options] [<application>]");
            Console.WriteLine("Runs <application> on an http server");
            Console.WriteLine("Example: OwinHost /p=5000 HelloWorld.Startup");
            Console.WriteLine();
            Console.WriteLine("Options:");

            foreach (CommandLineOption option in parser.Options)
            {
                Console.WriteLine(string.Format("   /{0} - {1}", option.Parameters.Aggregate((s1, s2) => s1 + ", /" + s2), option.Description));
            }

            Console.WriteLine();
            Console.WriteLine("Environment Variables:");
            Console.WriteLine("PORT                         Changes the default TCP port to listen on when");
            Console.WriteLine("                               both /port and /url options are not provided.");
            Console.WriteLine("OWIN_SERVER                  Changes the default server TYPE to use when");
            Console.WriteLine("                               the /server option is not provided.");
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
