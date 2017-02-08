// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.Owin.Hosting;

namespace FunctionalTests.Facts.OwinHost
{
    public class HostServer : IDisposable
    {
        private MyStartOptions startOptions;
        Process process = null;
        IDisposable serverObject = null;

        public HostServer(MyStartOptions startOptions)
        {
            this.startOptions = startOptions;
            this.Start();
        }

        private void Start()
        {
            if (startOptions.ExecuteInCommandLine)
            {
                string commandLineParameters = null;

                if (startOptions.DontPassStartupClassInCommandLine)
                {
                    commandLineParameters = startOptions.GetCommandLine();
                }
                else
                {
                    commandLineParameters = startOptions.FriendlyAppStartupName != null ?
                        string.Format("{0} \"{1}\"", startOptions.GetCommandLine(), startOptions.FriendlyAppStartupName) :
                        string.Format("{0} \"{1}\"", startOptions.GetCommandLine(), typeof(StartOptionsAndOwinHost).AssemblyQualifiedName);
                }

                process = new Process()
                {
                    StartInfo = new ProcessStartInfo(@"OwinHost.exe")
                        {
                            RedirectStandardError = true,
                            RedirectStandardInput = true,
                            UseShellExecute = false,
                            Arguments = commandLineParameters
                        }
                };

                Trace.WriteLine(string.Format("Executing Owinhost.exe {0}", process.StartInfo.Arguments));

                string line;
                if (!process.Start())
                {
                    Trace.WriteLine("OwinHost.exe failed to start");
                    while (!process.StandardError.EndOfStream && (line = process.StandardError.ReadLine()) != null)
                    {
                        Trace.WriteLine(line);
                    }
                }
            }
            else
            {
                serverObject = WebApp.Start<StartOptionsAndOwinHost>(startOptions);
            }
        }

        public void Dispose()
        {
            if (startOptions.ExecuteInCommandLine)
            {
                if (!process.HasExited)
                {
                    //Send enter to stop owinhost gracefully
                    process.StandardInput.WriteLine("close");
                    process.WaitForExit();
                }
            }
            else
            {
                serverObject.Dispose();
            }
        }
    }
}
