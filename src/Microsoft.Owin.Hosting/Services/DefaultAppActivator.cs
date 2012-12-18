using System;

namespace Microsoft.Owin.Hosting.Services
{
    public class DefaultAppActivator : IAppActivator
    {
        public static IAppActivator CreateInstance()
        {
            return new DefaultAppActivator();
        }

        public object Activate(Type type)
        {
            return Activator.CreateInstance(type);
        }
    }
}