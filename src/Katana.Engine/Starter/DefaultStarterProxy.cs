// <copyright file="DefaultStarterProxy.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
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
using System.IO;

namespace Katana.Engine.Starter
{
    public class DefaultStarterProxy : IKatanaStarter
    {
        public IDisposable Start(StartParameters parameters)
        {
            string directory = Directory.GetCurrentDirectory();
            var info = new AppDomainSetup
            {
                ApplicationBase = directory,
                PrivateBinPath = "bin",
                PrivateBinPathProbe = "*",
                ConfigurationFile = Path.Combine(directory, "web.config")
            };

            AppDomain domain = AppDomain.CreateDomain("OWIN", null, info);

            DefaultStarterAgent agent = CreateAgent(domain);

            agent.ResolveAssembliesFromDirectory(AppDomain.CurrentDomain.SetupInformation.ApplicationBase);

            return agent.Start(parameters);
        }

        private static DefaultStarterAgent CreateAgent(AppDomain domain)
        {
            try
            {
                return (DefaultStarterAgent)domain.CreateInstanceAndUnwrap(
                    typeof(DefaultStarterAgent).Assembly.FullName,
                    typeof(DefaultStarterAgent).FullName);
            }
            catch
            {
                return (DefaultStarterAgent)domain.CreateInstanceFromAndUnwrap(
                    typeof(DefaultStarterAgent).Assembly.Location,
                    typeof(DefaultStarterAgent).FullName);
            }
        }
    }
}
