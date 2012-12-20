using System;

namespace Microsoft.Owin.Hosting.Builder
{
    public interface IAppActivator
    {
        object Activate(Type type);
    }
}