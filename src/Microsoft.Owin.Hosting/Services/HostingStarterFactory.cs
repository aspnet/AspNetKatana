// <copyright file="HostingStarterFactory.cs" company="Microsoft Open Technologies, Inc.">
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
using System.IO;
using System.Linq;
using System.Reflection;

namespace Microsoft.Owin.Hosting.Services
{
    public class HostingStarterFactory : IHostingStarterFactory
    {
        private readonly IHostingStarterActivator _hostingStarterActivator;

        public HostingStarterFactory(IHostingStarterActivator hostingStarterActivator)
        {
            _hostingStarterActivator = hostingStarterActivator;
        }

        public virtual IHostingStarter Create(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return _hostingStarterActivator.Activate(typeof(DirectHostingStarter));
            }
            if (name == "Domain")
            {
                return _hostingStarterActivator.Activate(typeof(DomainHostingStarter));
            }

            // TODO: Is the attribute necessary? Can we load this using just a naming convention like we do for App and ServerFactory?
            Type hostingStarterType = LoadProvider(name)
                .GetCustomAttributes(inherit: false, attributeType: typeof(HostingStarterAttribute))
                .OfType<HostingStarterAttribute>()
                .Select(attribute => attribute.HostingStarterType)
                .SingleOrDefault();

            return _hostingStarterActivator.Activate(hostingStarterType);
        }

        private static Assembly LoadProvider(params string[] names)
        {
            var innerExceptions = new List<Exception>();
            foreach (var name in names)
            {
                try
                {
                    return Assembly.Load(name);
                }
                catch (FileNotFoundException ex)
                {
                    innerExceptions.Add(ex);
                }
                catch (FileLoadException ex)
                {
                    innerExceptions.Add(ex);
                }
                catch (BadImageFormatException ex)
                {
                    innerExceptions.Add(ex);
                }
            }
            throw new AggregateException(innerExceptions);
        }
    }
}
