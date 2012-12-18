using Owin;

namespace Microsoft.Owin.Hosting.Services
{
    public interface IAppBuilderFactory
    {
        IAppBuilder Create();
    }
}
