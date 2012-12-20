using System;

namespace Microsoft.Owin.Hosting.Starter
{
    public interface IHostingStarterActivator
    {
        IHostingStarter Activate(Type type);
    }
}