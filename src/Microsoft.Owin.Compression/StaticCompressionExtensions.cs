using System;
using Owin;

namespace Microsoft.Owin.Compression
{
    public static class StaticCompressionExtensions
    {
        public static IAppBuilder UseStaticCompression(this IAppBuilder builder)
        {
            return UseStaticCompression(builder, new StaticCompressionOptions());
        }

        public static IAppBuilder UseStaticCompression(this IAppBuilder builder, Action<StaticCompressionOptions> configure)
        {
            var options = new StaticCompressionOptions();
            configure(options);
            return UseStaticCompression(builder, options);
        }

        public static IAppBuilder UseStaticCompression(this IAppBuilder builder, StaticCompressionOptions options)
        {
            return builder.Use(typeof(StaticCompressionMiddleware), options);
        }
    }
}