// <copyright file="DomainHostingStarterAgent.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Lifetime;
using Microsoft.Owin.Hosting.Engine;
using Microsoft.Owin.Hosting.Services;

namespace Microsoft.Owin.Hosting.Starter
{
    /// <summary>
    /// Used for executing the IHostingEngine in a new AppDomain.
    /// </summary>
    public class DomainHostingStarterAgent : MarshalByRefObject, ISponsor, IDisposable
    {
        private ILease _lease;
        private bool _disposed;
        private IDisposable _runningApp;

        /// <summary>
        /// Registers a fallback assembly resolver that looks in the given directory.
        /// </summary>
        /// <param name="directory"></param>
        [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Reflection.Assembly.LoadFile", Justification = "By design")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Invoked cross domain")]
        public virtual void ResolveAssembliesFromDirectory(string directory)
        {
            var cache = new Dictionary<string, Assembly>();
            AppDomain.CurrentDomain.AssemblyResolve +=
                (a, b) =>
                {
                    Assembly assembly;
                    if (cache.TryGetValue(b.Name, out assembly))
                    {
                        return assembly;
                    }

                    string shortName = new AssemblyName(b.Name).Name;
                    string path = Path.Combine(directory, shortName + ".dll");
                    if (File.Exists(path))
                    {
                        assembly = Assembly.LoadFile(path);
                    }
                    cache[b.Name] = assembly;
                    if (assembly != null)
                    {
                        cache[assembly.FullName] = assembly;
                    }
                    return assembly;
                };
        }

        /// <summary>
        /// Executes the IHostingEngine in a new AppDomain.
        /// </summary>
        /// <param name="options"></param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Non-static needed for calling across AppDomain")]
        public virtual void Start(StartOptions options)
        {
            StartContext context = new StartContext(options);

            IServiceProvider services = ServicesFactory.Create(context.Options.Settings);

            IHostingEngine engine = services.GetService<IHostingEngine>();

            _runningApp = engine.Start(context);

            _lease = (ILease)RemotingServices.GetLifetimeService(this);
            _lease.Register(this);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _disposed = true;
                _lease.Unregister(this);
                _runningApp.Dispose();
            }
        }

        /// <summary>
        /// Renews the given lease for 5 minutes.
        /// </summary>
        /// <param name="lease"></param>
        /// <returns></returns>
        public virtual TimeSpan Renewal(ILease lease)
        {
            if (_disposed)
            {
                return TimeSpan.Zero;
            }

            return TimeSpan.FromMinutes(5);
        }
    }
}
