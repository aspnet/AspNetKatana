using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Owin;
using System.Threading.Tasks;

namespace Gate.Middleware
{
    /// <summary>
    /// Sets content type in response if none present
    /// </summary>
    internal static class ContentType
    {
        const string DefaultContentType = "text/html";

        public static IAppBuilder UseContentType(this IAppBuilder builder)
        {
            return builder.Use(Middleware);
        }

        public static IAppBuilder UseContentType(this IAppBuilder builder, string contentType)
        {
            return builder.Use(Middleware, contentType);
        }

        public static AppDelegate Middleware(AppDelegate app)
        {
            return Middleware(app, DefaultContentType);
        }

        public static AppDelegate Middleware(AppDelegate app, string contentType)
        {
            return call => app(call).Then(result =>
            {
                if (!result.Headers.HasHeader("Content-Type"))
                {
                    result.Headers.SetHeader("Content-Type", contentType);
                }
                return result;
            });
        }

    }
}