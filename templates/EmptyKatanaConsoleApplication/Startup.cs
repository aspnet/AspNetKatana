using Owin;

namespace $safeprojectname$
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseFileServer(options =>
            {
                options.EnableDirectoryBrowsing = true;
                options.StaticFileOptions.ServeUnknownFileTypes = true;
            });
        }
    }
}
