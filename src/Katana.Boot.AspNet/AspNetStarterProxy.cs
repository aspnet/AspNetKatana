// <copyright file="AspNetStarterProxy.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
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
// </copyright>

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Hosting;
using Katana.Boot.AspNet;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.Hosting.Services;
using Microsoft.Owin.Hosting.Utilities;

[assembly: HostingStarter(typeof(AspNetStarterProxy))]

namespace Katana.Boot.AspNet
{
    public class AspNetStarterProxy : MarshalByRefObject, IHostingStarter, IDisposable
    {
        private StartOptions _options;
        private IDisposable _running;

        public IDisposable Start(StartOptions options)
        {
            _options = options;
            StartDomain();
            return this;
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

            // TODO: parse _options.Url to find correct vdir
            var agent = (AspNetStarterAgent)ApplicationHost.CreateApplicationHost(
                typeof(AspNetStarterAgent),
                "/",
                directory);

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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Stop();
            }
        }
    }
}
