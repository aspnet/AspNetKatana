using Owin;
using Owin.Builder;

namespace Microsoft.Owin.Hosting.Services
{
    public class DefaultAppBuilderFactory : IAppBuilderFactory
    {
        public static IAppBuilderFactory CreateInstance()
        {
            return new DefaultAppBuilderFactory();
        }

        public IAppBuilder Create()
        {
            return new AppBuilder();
        }
    }
}