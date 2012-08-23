using System;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.WebApi.Owin;

namespace Owin
{
    public static class AppBuilderExtensions
    {
        public static IAppBuilder UseWebApi(this IAppBuilder builder, HttpRouteCollection routes)
        {
            return builder.UseType<HttpServerHandler>(routes);
        }

        public static IAppBuilder UseWebApi(this IAppBuilder builder, HttpConfiguration configuration)
        {
            return builder.UseType<HttpServerHandler>(configuration);
        }

        public static IAppBuilder UseWebApi(this IAppBuilder builder, HttpServer server)
        {
            return builder.UseType<HttpServerHandler>(server);
        }

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
