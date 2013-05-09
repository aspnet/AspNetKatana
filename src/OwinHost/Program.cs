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
using OwinHost.Options;

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
            Command command;
            try
            {
                command = CreateCommandModel().Parse(args);
            }
            catch (Exception e)
            {
                Console.WriteLine(Resources.ProgramOutput_CommandLineError, e.Message);
                Console.WriteLine();
                Console.WriteLine(Resources.ProgramOutput_Usage);
                ShowHelp(new Command { Model = CreateCommandModel() });
                return 1;
            }

            if (command.Run())
            {
                return 0;
            }
            return 1;
        }

        public static void RunServer(StartOptions options)
        {
            if (options == null)
            {
                return;
            }

            bool traceVerbose = IsVerboseTraceEnabled(options);
            WriteLine(options, traceVerbose, "Verbose");

            string boot;
            if (!options.Settings.TryGetValue("boot", out boot)
                || string.IsNullOrWhiteSpace(boot))
            {
                options.Settings["boot"] = "Domain";
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
                Console.WriteLine("{3} {0}{1}  {2}",
                    exception.GetType().FullName,
                    Environment.NewLine,
                    exception.Message,
                    Resources.ProgramOutput_ErrorTitle);
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
                    Console.WriteLine(Resources.ProgramOutput_PressCtrlCToTerminate);
                    e.Cancel = true;
                }
            };
        }

        public static CommandModel CreateCommandModel()
        {
            var model = new CommandModel();

            // run this alternate command for any help-like parameter
            model.Command("{show help}", IsHelpOption, (m, v) => { }).Execute(ShowHelp);

            // otherwise use these switches
            model.Option<StartOptions, string>(
                "server", "s", Resources.ProgramOutput_ServerOption,
                (options, value) => options.ServerFactory = value);

            model.Option<StartOptions, string>(
                "url", "u", Resources.ProgramOutput_UriOption,
                (options, value) => options.Urls.Add(value));

            model.Option<StartOptions, int>(
                "port", "p", Resources.ProgramOutput_PortOption,
                (options, value) => options.Port = value);

            model.Option<StartOptions, string>(
                "directory", "d", Resources.ProgramOutput_DirectoryOption,
                (options, value) => options.Settings["directory"] = value);

            model.Option<StartOptions, string>(
                "traceoutput", "o", Resources.ProgramOutput_OutputOption,
                (options, value) => options.Settings["traceoutput"] = value);

            model.Option<StartOptions, string>(
                "settings", Resources.ProgramOutput_SettingsOption,
                LoadSettings);

            model.Option<StartOptions, string>(
                "boot", "b", Resources.ProgramOutput_BootOption,
                (options, value) => options.Settings["boot"] = value);

            model.Option<StartOptions, string>(
                "verbosity", "v", Resources.ProgramOutput_VerbosityOption,
                (options, value) => options.Settings["traceverbosity"] = value);

            // and take the name of the application startup

            model.Parameter<string>((cmd, value) =>
            {
                var options = cmd.Get<StartOptions>();
                if (options.AppStartup == null)
                {
                    options.AppStartup = value;
                }
                else
                {
                    options.AppStartup += " " + value;
                }
            });

            // to call this action

            model.Execute<StartOptions>(RunServer);

            return model;
        }

        private static bool IsHelpOption(string s)
        {
            var helpOptions = new[] { "-?", "--?", "-h", "--h", "--help" };
            return helpOptions.Contains(s, StringComparer.OrdinalIgnoreCase);
        }

        private static void LoadSettings(StartOptions options, string settingsFile)
        {
            SettingsLoader.LoadFromSettingsFile(settingsFile, options.Settings);
        }

        private static void ShowHelp(Command cmd)
        {
            Console.WriteLine(Resources.ProgramOutput_Intro);
            Console.WriteLine();
            Console.WriteLine(Resources.ProgramOutput_UsageTemplate);
            Console.WriteLine();
            Console.WriteLine(Resources.ProgramOutput_Options);

            foreach (var option in cmd.Model.Root.Options)
            {
                string header;
                if (string.IsNullOrWhiteSpace(option.ShortName))
                {
                    header = "--" + option.Name;
                }
                else
                {
                    header = "-" + option.ShortName + ",--" + option.Name;
                }
                Console.WriteLine(FormatLines(header, option.Description));
            }

            Console.WriteLine();
            Console.WriteLine(Resources.ProgramOutput_EnvironmentVariablesHeader);
            Console.WriteLine(FormatLines("PORT", Resources.ProgramOutput_PortEnvironmentDescription));
            Console.WriteLine(FormatLines("OWIN_SERVER", Resources.ProgramOutput_ServerEnvironmentDescription));
            Console.WriteLine();
            Console.WriteLine(Resources.ProgramOutput_Example);
            Console.WriteLine();
        }

        public static string FormatLines(string header, string body)
        {
            string total = string.Empty;
            int lineLimit = 76;
            int offset = Math.Max(header.Length + 3, 20);

            string line = " " + header;

            while (offset + body.Length > lineLimit)
            {
                int bodyBreak = body.LastIndexOf(' ', lineLimit - offset);
                if (bodyBreak == -1)
                {
                    break;
                }
                total += line + new string(' ', offset - line.Length) + body.Substring(0, bodyBreak) + "\r\n";
                offset = 22;
                line = string.Empty;
                body = body.Substring(bodyBreak + 1);
            }
            return total + line + new string(' ', offset - line.Length) + body;
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
