using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Owin.Hosting.Services;

namespace Microsoft.Owin.Hosting.Starter
{
    public class DefaultHostingStarterFactory : IHostingStarterFactory
    {
        private readonly IHostingStarterActivator _hostingStarterActivator;

        public DefaultHostingStarterFactory(IHostingStarterActivator hostingStarterActivator)
        {
            _hostingStarterActivator = hostingStarterActivator;
        }

        public IHostingStarter Create(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return _hostingStarterActivator.Activate(typeof(DirectHostingStarter));
            }
            if (name == "Domain")
            {
                return _hostingStarterActivator.Activate(typeof(DomainHostingStarter));
            }

            Type hostingStarterType = LoadProvider(name)
                .GetCustomAttributes(inherit: false, attributeType: typeof(HostingStarterAttribute))
                .OfType<HostingStarterAttribute>()
                .Select(attribute=>attribute.HostingStarterType)
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
