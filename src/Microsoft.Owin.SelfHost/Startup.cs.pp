using Owin;

// This can be called from your Main method as follows:
// using (WebApp.Start<Startup>("http://localhost:12345"))
// {
//      Console.ReadLine();
// }

namespace $rootnamespace$
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
#if DEBUG
            app.UseErrorPage();
#endif
            app.UseWelcomePage("/");
        }
    }
}