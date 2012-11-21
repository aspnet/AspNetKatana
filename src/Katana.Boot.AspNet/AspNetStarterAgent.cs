// <copyright file="AspNetStarterAgent.cs" company="Katana contributors">
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
using System.Web.Hosting;
using Katana.Engine;
using Katana.Engine.Settings;
using Katana.Engine.Utilities;

namespace Katana.Boot.AspNet
{
    public class AspNetStarterAgent : MarshalByRefObject, IRegisteredObject
    {
        private AspNetStarterProxy _proxy;

        public IDisposable Start(AspNetStarterProxy proxy, StartParameters parameters)
        {
            _proxy = proxy;
            try
            {
                HostingEnvironment.RegisterObject(this);
            }
            catch
            {
                // Notification not always supported
            }

            var info = new StartContext
            {
                Parameters = parameters,
                Builder = new AppBuilderWrapper(),
            };

            IKatanaEngine engine = BuildEngine();

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
            string port = Environment.GetEnvironmentVariable("PORT", EnvironmentVariableTarget.Process);
            int portNumber;
            if (!string.IsNullOrWhiteSpace(port) && int.TryParse(port, out portNumber))
            {
                settings.DefaultPort = portNumber;
            }

            string owinServer = Environment.GetEnvironmentVariable("OWIN_SERVER", EnvironmentVariableTarget.Process);
            if (!string.IsNullOrWhiteSpace(owinServer))
            {
                settings.DefaultServer = owinServer;
            }
        }

        public void Stop(bool immediate)
        {
            try
            {
                HostingEnvironment.UnregisterObject(this);
            }
            catch
            {
                // ignored error
            }
            _proxy.Stop(immediate);
        }
    }
}
