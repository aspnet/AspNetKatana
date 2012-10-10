using Owin;

namespace Katana.Performance.ReferenceApp
{
    public class Startup
    {
        public void Configuration(IAppBuilder builder)
        {
            builder.UseType<CanonicalRequestPatterns>();
        }
    }
}