using System;

namespace Microsoft.Owin.Hosting.Services
{
    public static class HostingServiceProviderExtensions
    {
        public static T GetService<T>(this IServiceProvider services)
        {
            return (T)services.GetService(typeof(T));
        }
    }
}
