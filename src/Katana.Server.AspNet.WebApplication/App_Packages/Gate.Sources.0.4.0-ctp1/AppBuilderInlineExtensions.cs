using System;
using Owin;

namespace Gate
{
    internal static class AppBuilderInlineExtensions
    {
        public static IAppBuilder MapDirect(this IAppBuilder builder, string path, Action<Request, Response> app)
        {
            return builder.Map(path, map => map.RunDirect(app));
        }

        public static IAppBuilder RunDirect(this IAppBuilder builder, Action<Request, Response> app)
        {
            return builder.Run<AppDelegate>(() => (env, result, fault) => app(new Request(env), new Response(result)));
        }

        public static IAppBuilder UseDirect(this IAppBuilder builder, Action<Request, Response, Action> app)
        {
            return builder.Use<AppDelegate>(next => (env, result, fault) => app(new Request(env), new Response(result), () => next(env, result, fault)));
        }
    }
}
