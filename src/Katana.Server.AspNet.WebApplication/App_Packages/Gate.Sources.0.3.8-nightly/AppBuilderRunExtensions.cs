using System;
using System.Collections.Generic;
using System.Threading;
using Owin;

namespace Gate
{
#pragma warning disable 811
    using AppAction = Action< // app
       IDictionary<string, object>, // env
       Action< // result
           string, // status
           IDictionary<string, IEnumerable<string>>, // headers
           Action< // body
               Func< // write
                   ArraySegment<byte>, // data                     
                   bool>, // buffering
               Func< // flush
                   Action, // continuation
                   bool>, // async
               Action< // end
                   Exception>, // error
               CancellationToken>>, // cancel
       Action<Exception>>; // error

    internal static class AppBuilderRunExtensions
    {
        /*
         * Fundamental definition of Run.
         */

        public static IAppBuilder Run<TApp>(this IAppBuilder builder, Func<TApp> app)
        {
            return builder.Use<TApp>(_ => app());
        }

        /* 
         * Extension method to support passing in an already-built delegate.
         */

        public static IAppBuilder Run<TApp>(this IAppBuilder builder, TApp app)
        {
            return builder.Use<TApp>(_ => app);
        }

        /* 
         * Extension methods take a TApp factory func and its associated parameters.
         */

        public static IAppBuilder Run<TApp, T1>(this IAppBuilder builder, Func<T1, TApp> app, T1 arg1)
        {
            return builder.Use<TApp>(_ => app(arg1));
        }

        public static IAppBuilder Run<TApp, T1, T2>(this IAppBuilder builder, Func<T1, T2, TApp> app, T1 arg1, T2 arg2)
        {
            return builder.Use<TApp>(_ => app(arg1, arg2));
        }

        public static IAppBuilder Run<TApp, T1, T2, T3>(this IAppBuilder builder, Func<T1, T2, T3, TApp> app, T1 arg1, T2 arg2, T3 arg3)
        {
            return builder.Use<TApp>(_ => app(arg1, arg2, arg3));
        }

        public static IAppBuilder Run<TApp, T1, T2, T3, T4>(this IAppBuilder builder, Func<T1, T2, T3, T4, TApp> app, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            return builder.Use<TApp>(_ => app(arg1, arg2, arg3, arg4));
        }


        // strongly-typed to avoid "method group" issues when a class method is used
        public static IAppBuilder Run(this IAppBuilder builder, AppDelegate app)
        { return builder.Use<AppDelegate>(_ => app); }

        public static IAppBuilder Run(this IAppBuilder builder, Func<AppDelegate> app)
        { return builder.Use<AppDelegate>(_ => app()); }

        public static IAppBuilder Run<T1>(this IAppBuilder builder, Func<T1, AppDelegate> app, T1 arg1)
        { return builder.Use<AppDelegate>(_ => app(arg1)); }

        public static IAppBuilder Run<T1, T2>(this IAppBuilder builder, Func<T1, T2, AppDelegate> app, T1 arg1, T2 arg2)
        { return builder.Use<AppDelegate>(_ => app(arg1, arg2)); }

        public static IAppBuilder Run<T1, T2, T3>(this IAppBuilder builder, Func<T1, T2, T3, AppDelegate> app, T1 arg1, T2 arg2, T3 arg3)
        { return builder.Use<AppDelegate>(_ => app(arg1, arg2, arg3)); }

        public static IAppBuilder Run<T1, T2, T3, T4>(this IAppBuilder builder, Func<T1, T2, T3, T4, AppDelegate> app, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        { return builder.Use<AppDelegate>(_ => app(arg1, arg2, arg3, arg4)); }

        public static IAppBuilder Run(this IAppBuilder builder, AppAction app)
        { return builder.Use<AppAction>(_ => app); }
    }
}
