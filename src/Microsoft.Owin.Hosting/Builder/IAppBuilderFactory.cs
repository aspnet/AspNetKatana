using Owin;

namespace Microsoft.Owin.Hosting.Builder
{
    public interface IAppBuilderFactory
    {
        IAppBuilder Create();
    }
}
