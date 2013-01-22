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
using System.Diagnostics;

namespace CertInstaller
{
    internal enum InstallerCommand
    {
        Install,
        Uninstall
    }

    internal class Program
    {
        private static int Main(string[] args)
        {
            if (args.Length != 3)
            {
                PrintUsage();
                return 1;
            }

            try
            {
                InstallerCommand command = GetCommand(args[0]);
                string certName = args[1];
                string certPassword = args[2];

                var installer = new CertificateInstaller(certName, certPassword);

                switch (command)
                {
                    case InstallerCommand.Install:
                        installer.InstallCertificate();
                        break;
                    case InstallerCommand.Uninstall:
                        installer.UninstallCertificate();
                        break;
                    default:
                        Debug.Assert(false, String.Format("Unknown InstallerCommand value '{0}'", command));
                        break;
                }

                return 0;
            }
            catch (Exception e)
            {
                PrintUsage();

                Exception current = e;
                while (current != null)
                {
                    Console.WriteLine("{0}: {1}", current.GetType(), e.Message);
                    Console.WriteLine(e.StackTrace);
                    Console.WriteLine("------------------------------------------------------------------");
                    current = current.InnerException;
                }
                return 2;
            }
        }

        private static InstallerCommand GetCommand(string command)
        {
            if (string.IsNullOrEmpty(command))
            {
                throw new InvalidOperationException("Command must not be empty");
            }
            if (string.Compare(command, "install", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                return InstallerCommand.Install;
            }

            if (string.Compare(command, "uninstall", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                return InstallerCommand.Uninstall;
            }

            throw new InvalidOperationException(String.Format("Unknown command '{0}'", command));
        }

        private static void PrintUsage()
        {
            Console.WriteLine();
            Console.WriteLine("USAGE:");
            Console.WriteLine();
            Console.WriteLine("  CertInstaller.exe command certPath password");
            Console.WriteLine();
            Console.WriteLine(@"   command: either 'install' or 'uninstall'");
            Console.WriteLine(@"  certPath: Path to pfx file (e.g. c:\temp\certificate.pfx)");
            Console.WriteLine("  password: Password of pfx file.");
            Console.WriteLine();
        }
    }
}
