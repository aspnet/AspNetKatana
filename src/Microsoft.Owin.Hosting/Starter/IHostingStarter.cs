using System;

namespace Microsoft.Owin.Hosting.Starter
{
    public interface IHostingStarter
    {
        IDisposable Start(StartParameters parameters);
    }
}