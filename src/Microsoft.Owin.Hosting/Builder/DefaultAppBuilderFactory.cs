using Microsoft.Owin.Hosting.Services;
using Owin;
using Owin.Builder;

namespace Microsoft.Owin.Hosting.Builder
{
    public class DefaultAppBuilderFactory : IAppBuilderFactory
    {
        public IAppBuilder Create()
        {
            return new AppBuilder();
        }
    }
}