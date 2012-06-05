using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Owin;

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
            return (env, result, fault) => app(
                env,
                (status, headers, body) =>
                {
                    if (!headers.HasHeader("Content-Type"))
                    {
                        headers.SetHeader("Content-Type", contentType);
                    }

                    result(status, headers, body);
                },
                fault);
        }
    }
}