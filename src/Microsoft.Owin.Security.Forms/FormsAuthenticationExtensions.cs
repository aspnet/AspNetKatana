using System;
using Microsoft.Owin.Security.Forms;

namespace Owin
{
    public static class FormsAuthenticationExtensions
    {
        public static IAppBuilder UseFormsAuthentication(this IAppBuilder app, FormsAuthenticationOptions options)
        {
            app.Use(typeof(FormsAuthenticationMiddleware), options);
            return app;
        }

        public static IAppBuilder UseFormsAuthentication(this IAppBuilder app, Action<FormsAuthenticationOptions> configuration)
        {
            var options = new FormsAuthenticationOptions();
            configuration(options);
            return UseFormsAuthentication(app, options);
        }
    }
}