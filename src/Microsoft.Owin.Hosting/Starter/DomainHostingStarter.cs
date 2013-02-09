// <copyright file="DomainHostingStarter.cs" company="Katana contributors">
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace Microsoft.Owin.Hosting.Starter
{
    public class DomainHostingStarter : IHostingStarter
    {
        public IDisposable Start(StartOptions options)
        {
            string directory = Directory.GetCurrentDirectory();

            // If there are no /bin/ subdirs, and the current directory is called /bin/, move the current directory up one.
            // This fixes the case where a web app was run by katana.exe from the wrong directory.
            var directoryInfo = new DirectoryInfo(directory);
            if (directoryInfo.GetDirectories()
                .Where(subDirInfo => subDirInfo.Name.Equals("bin", StringComparison.OrdinalIgnoreCase)).Count() == 0
                && directoryInfo.Name.Equals("bin", StringComparison.OrdinalIgnoreCase))
            {
                directory = directoryInfo.Parent.FullName;
            }

            var info = new AppDomainSetup
            {
                ApplicationBase = directory,
                PrivateBinPath = "bin",
                PrivateBinPathProbe = "*",
                ConfigurationFile = Path.Combine(directory, "web.config")
            };

            AppDomain domain = AppDomain.CreateDomain("OWIN", null, info);

            DomainHostingStarterAgent agent = CreateAgent(domain);

            agent.ResolveAssembliesFromDirectory(AppDomain.CurrentDomain.SetupInformation.ApplicationBase);

            return agent.Start(options);
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
