using System;
using System.Linq;
using System.Net.Http;
using Katana.WebApi;
using Owin;

namespace Owin
{
    public static class AppBuilderExtensions
    {
        public static IAppBuilder UseMessageHandler(this IAppBuilder builder, DelegatingHandler handler)
        {
            return builder.UseMessageHandler(inner =>
                {
                    handler.InnerHandler = inner;
                    return handler;
                });
        }

        public static IAppBuilder UseMessageHandler(this IAppBuilder builder, Func<HttpMessageHandler, HttpMessageHandler> middleware)
        {
            AddConversions(builder);
            return builder.Use(middleware);
        }

        public static IAppBuilder UseMessageHandler<T>(this IAppBuilder builder, params object[] args) where T : HttpMessageHandler
        {
            AddConversions(builder);
            return builder.UseType<T>(args);
        }

        static void AddConversions(this IAppBuilder builder)
        {
            builder.AddSignatureConversion(Conversions.ToAppDelegate);
            builder.AddSignatureConversion(Conversions.ToMessageHandler);
        }
    }
}
