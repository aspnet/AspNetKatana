using Owin;

namespace $safeprojectname$
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseFileServer(opt => opt.WithDefaultContentType("application/octet-stream"));
        }
    }
}
