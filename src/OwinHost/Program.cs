﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.Hosting.Engine;
using Microsoft.Owin.Hosting.Services;
using Microsoft.Owin.Hosting.Starter;
using Microsoft.Owin.Hosting.Utilities;
using OwinHost.Options;
using System.Threading;

namespace OwinHost
{
    public static class Program
    {
        [LoaderOptimization(LoaderOptimization.MultiDomainHost)]
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
                if (e is CommandException || e is MissingMethodException || e is EntryPointNotFoundException)
                {
                    // these exception types are basic message errors
                    Console.WriteLine(Resources.ProgramOutput_CommandLineError, e.Message);
                    Console.WriteLine();
                    ShowHelp(new Command { Model = CreateCommandModel() });
                }
                else
                {
                    // otherwise let the exception terminate the process
                    throw;
                }
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

            string boot;
            if (!options.Settings.TryGetValue("boot", out boot)
                || string.IsNullOrWhiteSpace(boot))
            {
                options.Settings["boot"] = "Domain";
            }

            ResolveAssembliesFromDirectory(
                Path.Combine(Directory.GetCurrentDirectory(), "bin"));

            WriteLine("Starting with " + GetDisplayUrl(options));

            var done = new ManualResetEventSlim(false);
            AttachCtrlcSigtermShutdown(done);

            IServiceProvider services = ServicesFactory.Create();
            var starter = services.GetService<IHostingStarter>();
            IDisposable server = starter.Start(options);

            WriteLine("Started successfully");

            WriteLine("Press Ctrl+C to exit");
            done.Wait();

            WriteLine("Terminating.");

            server.Dispose();
        }

        private static void AttachCtrlcSigtermShutdown(ManualResetEventSlim resetEvent, string shutdownMessage = null)
        {
            Action shutdown = () =>
            {
                if (!string.IsNullOrEmpty(shutdownMessage))
                {
                    WriteLine(shutdownMessage);
                }
                resetEvent.Set();
            };

            AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) => shutdown();
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                shutdown();
                // Don't terminate the process immediately, wait for the Main thread to exit gracefully.
                eventArgs.Cancel = true;
            };
        }

        private static string GetDisplayUrl(StartOptions options)
        {
            IList<string> urls = options.Urls;
            if (urls.Count > 0)
            {
                return "urls: " + string.Join(", ", urls);
            }

            int port;
            string message = "port: ";
            if (!HostingEngine.TryDetermineCustomPort(options, out port))
            {
                port = HostingEngine.DefaultPort;
                message = "the default " + message;
            }

            return message + port + " (http://localhost:" + port + "/)";
        }

        private static void WriteLine(string data)
        {
            Console.WriteLine(data);
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
                Console.WriteLine(Resources.ProgramOutput_SimpleErrorMessage,
                    exception.GetType().FullName,
                    Environment.NewLine,
                    exception.Message);
                Display(exception.InnerException);
            }
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
            /* Disabled until we need to consume it anywhere.
            model.Option<StartOptions, string>(
                "verbosity", "v", "Set output verbosity level.",
                (options, value) => options.Settings["traceverbosity"] = value);
            */
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
            var helpOptions = new[] { "-?", "-h", "--help" };
            return helpOptions.Contains(s, StringComparer.OrdinalIgnoreCase);
        }

        private static void LoadSettings(StartOptions options, string settingsFile)
        {
            SettingsLoader.LoadFromSettingsFile(settingsFile, options.Settings);
        }

        private static void ShowHelp(Command cmd)
        {
            CommandModel rootCommand = cmd.Model.Root;

            string usagePattern = "OwinHost";
            foreach (var option in rootCommand.Options)
            {
                if (String.IsNullOrEmpty(option.ShortName))
                {
                    usagePattern += " [--" + option.Name + " VALUE]";
                }
                else
                {
                    usagePattern += " [-" + option.ShortName + " " + option.Name + "]";
                }
            }
            usagePattern += " [AppStartup]";

            Console.WriteLine(Resources.ProgramOutput_Intro);
            Console.WriteLine();
            Console.WriteLine(FormatLines(Resources.ProgramOutput_Usage, usagePattern, 0, 15));
            Console.WriteLine();
            Console.WriteLine(Resources.ProgramOutput_Options);

            foreach (var option in rootCommand.Options)
            {
                string header;
                if (string.IsNullOrWhiteSpace(option.ShortName))
                {
                    header = "  --" + option.Name;
                }
                else
                {
                    header = "  -" + option.ShortName + ",--" + option.Name;
                }
                Console.WriteLine(FormatLines(header, option.Description, 20, 2));
            }

            Console.WriteLine();
            Console.WriteLine(Resources.ProgramOutput_ParametersHeader);
            Console.WriteLine(FormatLines("  AppStartup", Resources.ProgramOutput_AppStartupParameter, 20, 2));
            Console.WriteLine();
            Console.WriteLine(FormatLines(string.Empty, Resources.ProgramOutput_AppStartupDescription, 2, 2));

            Console.WriteLine();
            Console.WriteLine(Resources.ProgramOutput_EnvironmentVariablesHeader);
            Console.WriteLine(FormatLines("  PORT", Resources.ProgramOutput_PortEnvironmentDescription, 20, 2));
            Console.WriteLine(FormatLines("  OWIN_SERVER", Resources.ProgramOutput_ServerEnvironmentDescription, 20, 2));
            Console.WriteLine();
            Console.WriteLine(Resources.ProgramOutput_Example);
            Console.WriteLine();
        }

        public static string FormatLines(string header, string body, int bodyOffset, int hangingIndent)
        {
            if (header == null)
            {
                throw new ArgumentNullException("header");
            }

            string total = string.Empty;
            int lineLimit = Console.WindowWidth - 2;
            int offset = Math.Max(header.Length + 2, bodyOffset);

            string line = header;

            while (offset + body.Length > lineLimit)
            {
                int bodyBreak = body.LastIndexOf(' ', lineLimit - offset);
                if (bodyBreak == -1)
                {
                    break;
                }
                total += line + new string(' ', offset - line.Length) + body.Substring(0, bodyBreak) + Environment.NewLine;
                offset = bodyOffset + hangingIndent;
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
