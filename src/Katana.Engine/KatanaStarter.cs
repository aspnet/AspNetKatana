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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Katana.Engine.Settings;
using Katana.Engine.Starter;

namespace Katana.Engine
{
    public class KatanaStarter : IKatanaStarter
    {
        public IDisposable Start(StartParameters parameters)
        {
            return String.IsNullOrWhiteSpace(parameters.Boot)
                ? DirectStart(parameters)
                : IndirectStart(parameters);
        }

        private IDisposable IndirectStart(StartParameters parameters)
        {
            var starter = BuildStarter(parameters.Boot);
            parameters.Boot = null;
            return starter.Start(parameters);
        }

        private Assembly LoadProvider(params string[] names)
        {
            var innerExceptions = new List<Exception>();
            foreach (var name in names)
            {
                try
                {
                    return Assembly.Load(name);
                }
                catch (Exception ex)
                {
                    innerExceptions.Add(ex);
                }
            }
            throw new AggregateException(innerExceptions);
        }

        private IKatanaStarter BuildStarter(string boot)
        {
            if (boot == "Default")
            {
                return new DefaultStarterProxy();
            }
            return LoadProvider("Katana.Boot." + boot, boot)
                .GetCustomAttributes(inherit: false)
                .OfType<IKatanaStarter>()
                .SingleOrDefault();
        }

        private static IDisposable DirectStart(StartParameters parameters)
        {
            var engine = BuildEngine();

            return engine.Start(new StartContext { Parameters = parameters });
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
