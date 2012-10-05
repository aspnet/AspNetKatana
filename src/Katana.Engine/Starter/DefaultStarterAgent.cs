// Copyright 2011-2012 Katana contributors
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
using Katana.Engine.CommandLine;
using Katana.Engine.Settings;
using Katana.Engine.Utils;

namespace Katana.Engine.Starter
{
    public class DefaultStarterAgent : MarshalByRefObject, IKatanaStarter
    {
        public void ResolveAssembliesFromDirectory(string directory)
        {
            DomainManager.ResolveAssembliesFromDirectory(directory);
        }

        public IDisposable Start(StartParameters parameters)
        {
            var info = new StartContext
                       {
                           Parameters = parameters,
                       };

            var engine = BuildEngine();

            return new Disposable(engine.Start(info).Dispose);
        }

        private static IKatanaEngine BuildEngine()
        {
            var settings = new KatanaSettings();
            TakeDefaultsFromEnvironment(settings);
            return new KatanaEngine(settings);
        }

        private static void TakeDefaultsFromEnvironment(KatanaSettings settings)
        {
            var port = Environment.GetEnvironmentVariable("PORT", EnvironmentVariableTarget.Process);
            int portNumber;
            if (!string.IsNullOrWhiteSpace(port) && int.TryParse(port, out portNumber))
            {
                settings.DefaultPort = portNumber;
            }

            var owinServer = Environment.GetEnvironmentVariable("OWIN_SERVER", EnvironmentVariableTarget.Process);
            if (!string.IsNullOrWhiteSpace(owinServer))
            {
                settings.DefaultServer = owinServer;
            }
        }
    }
}