// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace Microsoft.Owin.Hosting.Starter
{
    /// <summary>
    /// Creates a new AppDomain to run the IHostingEngine in.
    /// </summary>
    public class DomainHostingStarter : IHostingStarter
    {
        /// <summary>
        /// Creates a new AppDomain to run the IHostingEngine in.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed by caller")]
        public virtual IDisposable Start(StartOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            string directory;

            if (!options.Settings.TryGetValue("directory", out directory) || string.IsNullOrWhiteSpace(directory))
            {
                directory = Directory.GetCurrentDirectory();

                // If there are no /bin/ subdirs, and the current directory is called /bin/, move the current directory up one.
                // This fixes the case where a web app was run by katana.exe from the wrong directory.
                var directoryInfo = new DirectoryInfo(directory);
                if (directoryInfo.GetDirectories()
                                 .Where(subDirInfo => subDirInfo.Name.Equals("bin", StringComparison.OrdinalIgnoreCase)).Count() == 0
                    && directoryInfo.Name.Equals("bin", StringComparison.OrdinalIgnoreCase))
                {
                    directory = directoryInfo.Parent.FullName;
                }
            }

            var info = new AppDomainSetup
            {
                ApplicationBase = directory,
                PrivateBinPath = "bin",
                PrivateBinPathProbe = "*",
                LoaderOptimization = LoaderOptimization.MultiDomainHost,
                ConfigurationFile = Path.Combine(directory, "web.config")
            };

            AppDomain domain = AppDomain.CreateDomain("OWIN", null, info);

            DomainHostingStarterAgent agent = CreateAgent(domain);

            agent.ResolveAssembliesFromDirectory(AppDomain.CurrentDomain.SetupInformation.ApplicationBase);

            agent.Start(options);

            return agent;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Fallback code")]
        private static DomainHostingStarterAgent CreateAgent(AppDomain domain)
        {
            try
            {
                return (DomainHostingStarterAgent)domain.CreateInstanceAndUnwrap(
                    typeof(DomainHostingStarterAgent).Assembly.FullName,
                    typeof(DomainHostingStarterAgent).FullName);
            }
            catch
            {
                return (DomainHostingStarterAgent)domain.CreateInstanceFromAndUnwrap(
                    typeof(DomainHostingStarterAgent).Assembly.Location,
                    typeof(DomainHostingStarterAgent).FullName);
            }
        }
    }
}
