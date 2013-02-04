using Owin.Types;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace Owin
{
#region StartupExtensions

    // <summary>
    // Extension methods for IAppBuilder that provide syntax for commonly supported patterns.
    // </summary>
    internal static partial class StartupExtensions
    {
        // <summary>
        // Attach the given application to the pipeline.  Nothing attached after this point will be executed.
        // </summary>
        // <param name="builder"></param>
        // <param name="app"></param>
        public static void Run(this IAppBuilder builder, object app)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            builder.Use(new Func<object, object>(ignored => app));
        }

        // <summary>
        // The Build is called at the point when all of the middleware should be chained
        // together. May be called to build pipeline branches.
        // </summary>
        // <param name="builder"></param>
        // <returns>The request processing entry point for this section of the pipeline.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public static AppFunc Build(this IAppBuilder builder)
        {
            return builder.Build<AppFunc>();
        }

        // <summary>
        // The Build is called at the point when all of the middleware should be chained
        // together. May be called to build pipeline branches.
        // </summary>
        // <typeparam name="TApp">The application signature.</typeparam>
        // <param name="builder"></param>
        // <returns>The request processing entry point for this section of the pipeline.</returns>
        public static TApp Build<TApp>(this IAppBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            return (TApp)builder.Build(typeof(TApp));
        }

        // <summary>
        // Creates a new IAppBuilder instance from the current one and then invokes the configuration callback.
        // </summary>
        // <param name="builder"></param>
        // <param name="configuration">The callback for configuration.</param>
        // <returns>The request processing entry point for this section of the pipeline.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "By design")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public static AppFunc BuildNew(this IAppBuilder builder, Action<IAppBuilder> configuration)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            return builder.BuildNew<AppFunc>(configuration);
        }

        // <summary>
        // Creates a new IAppBuilder instance from the current one and then invokes the configuration callback.
        // </summary>
        // <typeparam name="TApp">The application signature.</typeparam>
        // <param name="builder"></param>
        // <param name="configuration">The callback for configuration.</param>
        // <returns>The request processing entry point for this section of the pipeline.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "By design")]
        public static TApp BuildNew<TApp>(this IAppBuilder builder, Action<IAppBuilder> configuration)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            IAppBuilder nested = builder.New();
            configuration(nested);
            return nested.Build<TApp>();
        }

        // <summary>
        // Adds converters for adapting between disparate application signatures.
        // </summary>
        // <param name="builder"></param>
        // <param name="conversion"></param>
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification = "False positive")]
        public static void AddSignatureConversion(
            this IAppBuilder builder,
            Delegate conversion)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            object value;
            if (builder.Properties.TryGetValue("builder.AddSignatureConversion", out value) &&
                value is Action<Delegate>)
            {
                ((Action<Delegate>)value).Invoke(conversion);
            }
            else
            {
                throw new MissingMethodException(builder.GetType().FullName, "AddSignatureConversion");
            }
        }
    }
#endregion

#region StartupExtensions.Func

    // <summary>
    // Extension methods for IAppBuilder that provide syntax for commonly supported patterns.
    // </summary>
    internal static partial class StartupExtensions
    {
        // <summary>
        // Specifies a middleware instance generator of the given type.
        // </summary>
        // <typeparam name="TApp">The applicaiton signature.</typeparam>
        // <param name="builder"></param>
        // <param name="middleware">A Func that generates a middleware instance given a reference to the next middleware.</param>
        // <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public static IAppBuilder UseFunc<TApp>(this IAppBuilder builder, Func<TApp, TApp> middleware)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            return builder.Use(middleware);
        }

        // <summary>
        // Specifies a middleware instance generator.
        // </summary>
        // <param name="builder"></param>
        // <param name="middleware">A Func that generates a middleware instance given a reference to the next middleware.</param>
        // <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public static IAppBuilder UseFunc(this IAppBuilder builder, Func<AppFunc, AppFunc> middleware)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            return builder.Use(middleware);
        }

        // <summary>
        // Specifies a middleware instance generator that takes one additional argument.
        // </summary>
        // <typeparam name="T1">The Type of the given extra argument.</typeparam>
        // <param name="builder"></param>
        // <param name="middleware">A Func that generates a middleware instance given a reference to the next middleware and any extra arguments.</param>
        // <param name="arg1">Extra arguments for the middleware generator.</param>
        // <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public static IAppBuilder UseFunc<T1>(this IAppBuilder builder, Func<AppFunc, T1, AppFunc> middleware, T1 arg1)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            return builder.UseFunc<AppFunc>(app => middleware(app, arg1));
        }

        // <summary>
        // Specifies a middleware instance generator that takes two additional arguments.
        // </summary>
        // <typeparam name="T1">The Type of the given extra argument.</typeparam>
        // <typeparam name="T2">The Type of the given extra argument.</typeparam>
        // <param name="builder"></param>
        // <param name="middleware">A Func that generates a middleware instance given a reference to the next middleware and any extra arguments.</param>
        // <param name="arg1">Extra arguments for the middleware generator.</param>
        // <param name="arg2">Extra arguments for the middleware generator.</param>
        // <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public static IAppBuilder UseFunc<T1, T2>(this IAppBuilder builder, Func<AppFunc, T1, T2, AppFunc> middleware, T1 arg1, T2 arg2)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            return builder.UseFunc<AppFunc>(app => middleware(app, arg1, arg2));
        }

        // <summary>
        // Specifies a middleware instance generator that takes three additional arguments.
        // </summary>
        // <typeparam name="T1">The Type of the given extra argument.</typeparam>
        // <typeparam name="T2">The Type of the given extra argument.</typeparam>
        // <typeparam name="T3">The Type of the given extra argument.</typeparam>
        // <param name="builder"></param>
        // <param name="middleware">A Func that generates a middleware instance given a reference to the next middleware and any extra arguments.</param>
        // <param name="arg1">Extra arguments for the middleware generator.</param>
        // <param name="arg2">Extra arguments for the middleware generator.</param>
        // <param name="arg3">Extra arguments for the middleware generator.</param>
        // <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public static IAppBuilder UseFunc<T1, T2, T3>(this IAppBuilder builder, Func<AppFunc, T1, T2, T3, AppFunc> middleware, T1 arg1, T2 arg2, T3 arg3)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            return builder.UseFunc<AppFunc>(app => middleware(app, arg1, arg2, arg3));
        }

        // <summary>
        // Specifies a middleware instance generator that takes four additional arguments.
        // </summary>
        // <typeparam name="T1">The Type of the given extra argument.</typeparam>
        // <typeparam name="T2">The Type of the given extra argument.</typeparam>
        // <typeparam name="T3">The Type of the given extra argument.</typeparam>
        // <typeparam name="T4">The Type of the given extra argument.</typeparam>
        // <param name="builder"></param>
        // <param name="middleware">A Func that generates a middleware instance given a reference to the next middleware and any extra arguments.</param>
        // <param name="arg1">Extra arguments for the middleware generator.</param>
        // <param name="arg2">Extra arguments for the middleware generator.</param>
        // <param name="arg3">Extra arguments for the middleware generator.</param>
        // <param name="arg4">Extra arguments for the middleware generator.</param>
        // <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public static IAppBuilder UseFunc<T1, T2, T3, T4>(this IAppBuilder builder, Func<AppFunc, T1, T2, T3, T4, AppFunc> middleware, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            return builder.UseFunc<AppFunc>(app => middleware(app, arg1, arg2, arg3, arg4));
        }

        // <summary>
        // Specifies a middleware instance generator that takes one additional argument.
        // </summary>
        // <typeparam name="T1">The Type of the given extra argument.</typeparam>
        // <param name="builder"></param>
        // <param name="middleware">A Func that generates a middleware instance given a reference to the next middleware and any extra arguments.</param>
        // <param name="arg1">Extra arguments for the middleware generator.</param>
        // <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public static IAppBuilder UseFunc<T1>(this IAppBuilder builder, Func<T1, Func<AppFunc, AppFunc>> middleware, T1 arg1)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            return builder.UseFunc<AppFunc>(app => middleware(arg1)(app));
        }

        // <summary>
        // Specifies a middleware instance generator that takes two additional arguments.
        // </summary>
        // <typeparam name="T1">The Type of the given extra argument.</typeparam>
        // <typeparam name="T2">The Type of the given extra argument.</typeparam>
        // <param name="builder"></param>
        // <param name="middleware">A Func that generates a middleware instance given a reference to the next middleware and any extra arguments.</param>
        // <param name="arg1">Extra arguments for the middleware generator.</param>
        // <param name="arg2">Extra arguments for the middleware generator.</param>
        // <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public static IAppBuilder UseFunc<T1, T2>(this IAppBuilder builder, Func<T1, T2, Func<AppFunc, AppFunc>> middleware, T1 arg1, T2 arg2)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            return builder.UseFunc<AppFunc>(app => middleware(arg1, arg2)(app));
        }

        // <summary>
        // Specifies a middleware instance generator that takes three additional arguments.
        // </summary>
        // <typeparam name="T1">The Type of the given extra argument.</typeparam>
        // <typeparam name="T2">The Type of the given extra argument.</typeparam>
        // <typeparam name="T3">The Type of the given extra argument.</typeparam>
        // <param name="builder"></param>
        // <param name="middleware">A Func that generates a middleware instance given a reference to the next middleware and any extra arguments.</param>
        // <param name="arg1">Extra arguments for the middleware generator.</param>
        // <param name="arg2">Extra arguments for the middleware generator.</param>
        // <param name="arg3">Extra arguments for the middleware generator.</param>
        // <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public static IAppBuilder UseFunc<T1, T2, T3>(this IAppBuilder builder, Func<T1, T2, T3, Func<AppFunc, AppFunc>> middleware, T1 arg1, T2 arg2, T3 arg3)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            return builder.UseFunc<AppFunc>(app => middleware(arg1, arg2, arg3)(app));
        }

        // <summary>
        // Specifies a middleware instance generator that takes four additional arguments.
        // </summary>
        // <typeparam name="T1">The Type of the given extra argument.</typeparam>
        // <typeparam name="T2">The Type of the given extra argument.</typeparam>
        // <typeparam name="T3">The Type of the given extra argument.</typeparam>
        // <typeparam name="T4">The Type of the given extra argument.</typeparam>
        // <param name="builder"></param>
        // <param name="middleware">A Func that generates a middleware instance given a reference to the next middleware and any extra arguments.</param>
        // <param name="arg1">Extra arguments for the middleware generator.</param>
        // <param name="arg2">Extra arguments for the middleware generator.</param>
        // <param name="arg3">Extra arguments for the middleware generator.</param>
        // <param name="arg4">Extra arguments for the middleware generator.</param>
        // <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public static IAppBuilder UseFunc<T1, T2, T3, T4>(this IAppBuilder builder, Func<T1, T2, T3, T4, Func<AppFunc, AppFunc>> middleware, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            return builder.UseFunc<AppFunc>(app => middleware(arg1, arg2, arg3, arg4)(app));
        }
    }
#endregion

#region StartupExtensions.OwinTypes

    internal static partial class StartupExtensions
    {
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public static IAppBuilder UseOwin(
            this IAppBuilder builder,
            Func<OwinRequest, OwinResponse, Func<Task>, Task> process)
        {
            return builder.UseFunc(
                next => env => process(
                    new OwinRequest(env),
                    new OwinResponse(env),
                    () => next(env)));
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public static IAppBuilder UseOwin(
            this IAppBuilder builder,
            Action<OwinRequest> process)
        {
            return builder.UseFunc(
                next => env =>
                {
                    process(new OwinRequest(env));
                    return next(env);
                });
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public static IAppBuilder UseOwin(
            this IAppBuilder builder,
            Func<OwinRequest, Task> process)
        {
            return builder.UseFunc(
                next => env =>
                {
                    var task = process(new OwinRequest(env));
                    if (task.IsCompleted)
                    {
                        if (task.IsFaulted || task.IsCanceled)
                        {
                            return task;
                        }
                        return next(env);
                    }
                    return task.ContinueWith(t => next(env), TaskContinuationOptions.OnlyOnRanToCompletion);
                });
        }
    }
#endregion

#region StartupExtensions.Type

    // <summary>
    // Extension methods for IAppBuilder that provide syntax for commonly supported patterns.
    // </summary>
    internal static partial class StartupExtensions
    {
        // <summary>
        // Adds an instance of the given middleware type to the pipeline using the constructor that takes
        // an application delegate as the first parameter and the given params args for any remaining parameters.
        // </summary>
        // <typeparam name="TMiddleware">The Type of the middleware to construct.</typeparam>
        // <param name="builder"></param>
        // <param name="args">Any additional arguments to pass into the middleware constructor.</param>
        // <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "By design")]
        public static IAppBuilder UseType<TMiddleware>(this IAppBuilder builder, params object[] args)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            return builder.Use(typeof(TMiddleware), args);
        }

        // <summary>
        // Adds an instance of the given middleware type to the pipeline using the constructor that takes
        // an application delegate as the first parameter and the given params args for any remaining parameters.
        // </summary>
        // <param name="builder"></param>
        // <param name="type">The Type of the middleware to construct.</param>
        // <param name="args">Any additional arguments to pass into the middleware constructor.</param>
        // <returns></returns>
        public static IAppBuilder UseType(this IAppBuilder builder, Type type, params object[] args)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            return builder.Use(type, args);
        }
    }
#endregion

}
