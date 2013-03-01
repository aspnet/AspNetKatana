// ReSharper disable CheckNamespace

using Microsoft.Owin.Throttling;

namespace Owin
{
    public static class ThrottlingExtensions
    {
        public static IAppBuilder UseThrottling(this IAppBuilder builder, ThrottlingOptions options)
        {
            return builder.Use(typeof(ThrottlingMiddleware), options);
        }
    }
}
