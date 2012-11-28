// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics;

namespace CertInstaller
{
    internal enum InstallerCommand
    { 
        Install,
        Uninstall
    }

    public class Program
    {
        public static int Main(string[] args)
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

                CertificateInstaller installer = new CertificateInstaller(certName, certPassword);

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
