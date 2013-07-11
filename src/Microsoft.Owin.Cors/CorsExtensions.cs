using System;
using System.Diagnostics.CodeAnalysis;
using System.Web.Cors;
using Microsoft.Owin.Cors;
using Owin;

namespace Owin
{
    public static class CorsExtensions
    {
        public static IAppBuilder UseCors(this IAppBuilder builder, CorsOptions corsOptions)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            if (corsOptions == null)
            {
                throw new ArgumentNullException("corsOptions");
            }

            return builder.Use(typeof(CorsMiddleware), corsOptions);
        }
    }
}
