using System;
using Microsoft.Owin.Hosting.Services;

namespace Microsoft.Owin.Hosting.Builder
{
    public class DefaultAppActivator : IAppActivator
    {
        private readonly IServiceProvider _services;

        public DefaultAppActivator(IServiceProvider services)
        {
            _services = services;
        }

        public object Activate(Type type)
        {
            try
            {
                var starter = _services.GetService(type);
                if (starter != null)
                {
                    return starter;
                }
            }
            catch
            {
            }
            return ActivatorUtils.CreateInstance(_services, type);
        }
    }
}