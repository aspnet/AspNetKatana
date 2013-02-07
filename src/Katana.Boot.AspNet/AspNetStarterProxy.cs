// <copyright file="AspNetStarterProxy.cs" company="Katana contributors">
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
using System.Threading;
using System.Web.Hosting;
using Katana.Boot.AspNet;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.Hosting.Starter;
using Microsoft.Owin.Hosting.Utilities;

[assembly: HostingStarter(typeof(AspNetStarterProxy))]

namespace Katana.Boot.AspNet
{
    public class AspNetStarterProxy : MarshalByRefObject, IHostingStarter
    {
        private StartOptions _options;
        private IDisposable _running;

        public IDisposable Start(StartOptions options)
        {
            _options = options;
            StartDomain();
            return new Disposable(Stop);
        }

        private void Stop()
        {
            IDisposable running = Interlocked.Exchange(ref _running, null);
            if (running != null)
            {
                running.Dispose();
            }
        }

        private void StartDomain()
        {
            var agent = (AspNetStarterAgent)ApplicationHost.CreateApplicationHost(
                typeof(AspNetStarterAgent),
                _options.Path ?? "/",
                Directory.GetCurrentDirectory());

            IDisposable running = agent.Start(this, _options);
            IDisposable prior = Interlocked.Exchange(ref _running, running);
            if (prior != null)
            {
                // TODO: UNEXPECTED
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "immediate", Justification = "Interface method")]
        public void StopDomain(bool immediate)
        {
            IDisposable running = Interlocked.Exchange(ref _running, null);
            if (running != null)
            {
                running.Dispose();

                // ASP.NET has indicated a Stop when the starter 
                // believes it is still running. After the the old 
                // agent is disposed, Start is called to re-create a 
                // replacement app domain.
                StartDomain();
            }
        }
    }
}
