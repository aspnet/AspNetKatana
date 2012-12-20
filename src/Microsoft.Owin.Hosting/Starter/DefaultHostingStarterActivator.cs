using System;
using Microsoft.Owin.Hosting.Services;

namespace Microsoft.Owin.Hosting.Starter
{
    public class DefaultHostingStarterActivator : IHostingStarterActivator
    {
        private readonly IServiceProvider _services;

        public DefaultHostingStarterActivator(IServiceProvider services)
        {
            _services = services;
        }

        public IHostingStarter Activate(Type type)
        {
            try
            {
                var starter = (IHostingStarter)_services.GetService(type);
                if (starter != null)
                {
                    return starter;
                }
            }
            catch
            {
            }
            return (IHostingStarter)ActivatorUtils.CreateInstance(_services, type);
        }
    }
}