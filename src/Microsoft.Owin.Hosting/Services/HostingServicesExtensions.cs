using System;

namespace Microsoft.Owin.Hosting.Services
{
    public static class ServiceProviderExtensions
    {
        public static T GetService<T>(this IServiceProvider services)
        {
            return (T)services.GetService(typeof(T));
        }
    }
}
