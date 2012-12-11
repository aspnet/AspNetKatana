// <copyright file="AspNetStarterProxy.cs" company="Katana contributors">
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Web.Hosting;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.Hosting.Utilities;

namespace Katana.Boot.AspNet
{
    public class AspNetStarterProxy : MarshalByRefObject
    {
        private StartParameters _parameters;
        private IDisposable _running;

        public IDisposable StartKatana(StartParameters parameters)
        {
            _parameters = parameters;
            Start();
            return new Disposable(StopKatana);
        }

        private void StopKatana()
        {
            IDisposable running = Interlocked.Exchange(ref _running, null);
            if (running != null)
            {
                running.Dispose();
            }
        }

        private void Start()
        {
            var starter = (AspNetStarterAgent)ApplicationHost.CreateApplicationHost(
                typeof(AspNetStarterAgent),
                _parameters.Path ?? "/",
                Directory.GetCurrentDirectory());

            IDisposable running = starter.Start(this, _parameters);
            IDisposable prior = Interlocked.Exchange(ref _running, running);
            if (prior != null)
            {
                // TODO: UNEXPECTED
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "immediate", Justification = "Interface method")]
        public void Stop(bool immediate)
        {
            IDisposable running = Interlocked.Exchange(ref _running, null);
            if (running != null)
            {
                running.Dispose();

                // ASP.NET has indicated a Stop when the starter 
                // believes it is still running. After the the old 
                // agent is disposed, Start is called to re-create a 
                // replacement app domain.
                Start();
            }
        }
    }
}
