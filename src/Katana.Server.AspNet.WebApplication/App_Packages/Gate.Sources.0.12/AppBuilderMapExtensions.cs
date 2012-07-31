using System;
using Gate.Mapping;
using Owin;

namespace Gate
{
    internal static class AppBuilderMapExtensions
    {
        /*
         * Fundamental definition of Map.
         */

        public static IAppBuilder Map(this IAppBuilder builder, string path, AppDelegate app)
        {
            var mapBuilder = builder as MapBuilder ?? new MapBuilder(builder, UrlMapper.Create);
            mapBuilder.MapInternal(path, app);
            return mapBuilder;
        }

        /*
         * Extension to allow branching of AppBuilder.
         */

        public static IAppBuilder Map(this IAppBuilder builder, string path, Action<IAppBuilder> app)
        {
            return builder.Map(path, builder.Build<AppDelegate>(app));
        }

        /*
         * Extensions to map AppDelegate factory func to a given path, with optional parameters.
         */

        public static IAppBuilder Map(this IAppBuilder builder, string path, Func<AppDelegate> app)
        {
            return builder.Map(path, b2 => b2.Run(app));
        }

        public static IAppBuilder Map<T1>(this IAppBuilder builder, string path, Func<T1, AppDelegate> app, T1 arg1)
        {
            return builder.Map(path, b2 => b2.Run(app, arg1));
        }

        public static IAppBuilder Map<T1, T2>(this IAppBuilder builder, string path, Func<T1, T2, AppDelegate> app, T1 arg1, T2 arg2)
        {
            return builder.Map(path, b2 => b2.Run(app, arg1, arg2));
        }

        public static IAppBuilder Map<T1, T2, T3>(this IAppBuilder builder, string path, Func<T1, T2, T3, AppDelegate> app, T1 arg1, T2 arg2, T3 arg3)
        {
            return builder.Map(path, b2 => b2.Run(app, arg1, arg2, arg3));
        }

        public static IAppBuilder Map<T1, T2, T3, T4>(this IAppBuilder builder, string path, Func<T1, T2, T3, T4, AppDelegate> app, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            return builder.Map(path, b2 => b2.Run(app, arg1, arg2, arg3, arg4));
        }
    }
}