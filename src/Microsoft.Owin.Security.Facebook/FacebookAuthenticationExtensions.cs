using System;
using Microsoft.Owin.Security.Facebook;

namespace Owin
{
    public static class FacebookAuthenticationExtensions
    {
        public static IAppBuilder UseFacebookAuthentication(this IAppBuilder app, FacebookAuthenticationOptions options)
        {
            app.Use(typeof(FacebookAuthenticationMiddleware), options);
            return app;
        }
    }
}