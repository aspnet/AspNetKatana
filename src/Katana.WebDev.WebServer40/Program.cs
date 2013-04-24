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
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security;
using System.Text;
using System.Threading;
using Katana.WebDev.WebServer40.Options;
using Microsoft.Owin.Hosting;
using Microsoft.Win32;

namespace Katana.WebDev.WebServer40
{
    public class Program
    {
        private static int Main(string[] args)
        {
            foreach (var arg in args)
            {
                Console.WriteLine("[{0}]", arg);
            }

            int exitCode = 0;
            try
            {
                exitCode = Run(args);
            }
            catch (Exception ex)
            {
                Display(ex);
                Console.WriteLine("Press enter to exit");
                exitCode = 1;
            }
            finally
            {
                Environment.Exit(exitCode);
            }
            return exitCode;
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

        public static int Run(string[] args)
        {
            CommandModel model = CreateCommandModel();
            Command command = model.Parse(args);
            if (command.Run())
            {
                return command.Get<int>();
            }
            Console.WriteLine("Unrecognized command");
            return 1;
        }

        private static void RunInstall(Command cmd)
        {
            try
            {
                RegistryKey subKey = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Image File Execution Options\\webdev.webserver40.exe");
                subKey.SetValue("debugger", Process.GetCurrentProcess().MainModule.FileName);
            }
            catch (UnauthorizedAccessException)
            {
                if (cmd.Parameters.Count != 0)
                {
                    throw;
                }

                RunElevated();
            }
            catch (SecurityException)
            {
                if (cmd.Parameters.Count != 0)
                {
                    throw;
                }

                RunElevated();
            }
        }

        private static void RunUninstall(Command cmd)
        {
            try
            {
                RegistryKey subKey = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Image File Execution Options\\webdev.webserver40.exe");
                subKey.DeleteValue("debugger");
            }
            catch (UnauthorizedAccessException)
            {
                if (cmd.Parameters.Count != 0)
                {
                    throw;
                }

                RunElevated();
            }
            catch (SecurityException)
            {
                if (cmd.Parameters.Count != 0)
                {
                    throw;
                }

                RunElevated();
            }
        }

        private static void RunElevated()
        {
            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = true,
                Verb = "runas",
                WorkingDirectory = Environment.CurrentDirectory,
                FileName = Process.GetCurrentProcess().MainModule.FileName,
                Arguments = Environment.CommandLine + " elevated"
            };
            if (startInfo.Arguments.StartsWith(startInfo.FileName, StringComparison.OrdinalIgnoreCase))
            {
                startInfo.Arguments = startInfo.Arguments.Substring(startInfo.FileName.Length);
            }
            else if (startInfo.Arguments.StartsWith("\"" + startInfo.FileName + "\"", StringComparison.OrdinalIgnoreCase))
            {
                startInfo.Arguments = startInfo.Arguments.Substring(startInfo.FileName.Length + 2);
            }
            Process process = Process.Start(startInfo);
            process.WaitForExit();
        }

        public static int RunWebApplication(StartOptions options)
        {
            var redirectEvent = new ManualResetEvent(false);
            var terminateEvent = new ManualResetEvent(false);
            int connections = 0;

            bool[] closed = { false };
            var listener = new TcpListener(IPAddress.Loopback, options.Port.Value);
            listener.Start();

            AsyncCallback onAccept = null;
            onAccept = ar =>
            {
                try
                {
                    Interlocked.Increment(ref connections);
                    if (closed[0])
                    {
                        return;
                    }
                    Socket socket = listener.EndAcceptSocket(ar);
                    Console.Write("+");
                    listener.BeginAcceptSocket(onAccept, null);
                    redirectEvent.WaitOne();

                    socket.Send(Encoding.UTF8.GetBytes(string.Format(@"HTTP/1.0 301 Awful Hack
Location: http://localhost:{0}/
Pragma: no-cache
Cache-Control: private
Connection: close

", options.Port.Value)));
                    while (socket.Receive(new byte[1024]) != 0)
                    {
                        // waiting for disconnect
                    }
                    socket.Close();
                }
                catch (Exception)
                {
                    // Console.WriteLine(ex.Message);
                }
            };
            listener.BeginAcceptSocket(onAccept, null);

            DateTime patience = DateTime.UtcNow.AddSeconds(45);
            Console.Write("Waiting for debugger");
            while ((!Debugger.IsAttached || connections < 2) &&
                patience > DateTime.UtcNow)
            {
                Console.Write(".");
                Thread.Sleep(750);
            }
            Console.WriteLine(Debugger.IsAttached ? "!" : "?");
            var timer = new System.Timers.Timer(750);
            timer.Elapsed += (a, b) =>
            {
                if (!Debugger.IsAttached)
                {
                    terminateEvent.Set();
                }
            };
            if (Debugger.IsAttached)
            {
                timer.Start();
            }

            if (string.IsNullOrWhiteSpace(options.Boot))
            {
                options.Boot = "Domain";
            }
            if (options.Verbosity == 0)
            {
                options.Verbosity = 1;
            }
            if (!string.IsNullOrWhiteSpace(options.Directory))
            {
                // TODO: remove 
                Directory.SetCurrentDirectory(options.Directory);
            }

            closed[0] = true;
            listener.Stop();
            using (WebApp.Start(options))
            {
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    Console.ReadLine();
                    terminateEvent.Set();
                });
                redirectEvent.Set();
                terminateEvent.WaitOne();
            }
            timer.Stop();
            return 0;
        }

        public static CommandModel CreateCommandModel()
        {
            var model = new CommandModel();
            model.Command("install")
                .Parameter<string>((m, v) => m.Parameters.Add(v))
                .Execute(RunInstall);

            model.Command("uninstall")
                .Parameter<string>((m, v) => m.Parameters.Add(v))
                .Execute(RunUninstall);

            model.Command("{run server}", EndingWithExe, (m, v) => m.Set(v))
                .Option<StartOptions, int>("port", (m, v) => m.Port = v)
                .Option<StartOptions, string>("path", (m, v) => m.Directory = v)
                .Option<StartOptions, string>("vpath", (m, v) => { })
                .Execute<StartOptions, int>(RunWebApplication);

            return model;
        }

        private static bool EndingWithExe(string parameter)
        {
            return parameter.EndsWith(".exe", StringComparison.OrdinalIgnoreCase);
        }
    }
}
