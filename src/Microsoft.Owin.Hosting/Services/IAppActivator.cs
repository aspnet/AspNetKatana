using System;

namespace Microsoft.Owin.Hosting.Services
{
    public interface IAppActivator
    {
        object Activate(Type type);
    }
}