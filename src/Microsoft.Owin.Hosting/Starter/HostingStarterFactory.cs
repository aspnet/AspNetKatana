// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Microsoft.Owin.Hosting.Starter
{
    /// <summary>
    /// Selects from known hosting starters, or detects additional providers via convention.
    /// </summary>
    public class HostingStarterFactory : IHostingStarterFactory
    {
        private readonly IHostingStarterActivator _hostingStarterActivator;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hostingStarterActivator"></param>
        public HostingStarterFactory(IHostingStarterActivator hostingStarterActivator)
        {
            _hostingStarterActivator = hostingStarterActivator;
        }

        /// <summary>
        /// Selects from known hosting starters, or detects additional providers via convention.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
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
