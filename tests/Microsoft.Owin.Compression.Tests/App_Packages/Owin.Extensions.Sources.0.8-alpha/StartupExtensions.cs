using Owin.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace Owin
{
#region StartupExtensions

    internal static partial class StartupExtensions
    {
        public static void Run(this IAppBuilder builder, object app)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            builder.Use(new Func<object, object>(ignored => app));
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public static AppFunc Build(this IAppBuilder builder)
        {
            return builder.Build<AppFunc>();
        }

        public static TApp Build<TApp>(this IAppBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            return (TApp)builder.Build(typeof(TApp));
        }

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

    internal static partial class StartupExtensions
    {
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public static IAppBuilder UseFunc<TApp>(this IAppBuilder builder, Func<TApp, TApp> middleware)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            return builder.Use(middleware);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public static IAppBuilder UseFunc(this IAppBuilder builder, Func<AppFunc, AppFunc> middleware)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            return builder.Use(middleware);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public static IAppBuilder UseFunc<T1>(this IAppBuilder builder, Func<AppFunc, T1, AppFunc> middleware, T1 arg1)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            return builder.UseFunc<AppFunc>(app => middleware(app, arg1));
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public static IAppBuilder UseFunc<T1, T2>(this IAppBuilder builder, Func<AppFunc, T1, T2, AppFunc> middleware, T1 arg1, T2 arg2)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            return builder.UseFunc<AppFunc>(app => middleware(app, arg1, arg2));
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public static IAppBuilder UseFunc<T1, T2, T3>(this IAppBuilder builder, Func<AppFunc, T1, T2, T3, AppFunc> middleware, T1 arg1, T2 arg2, T3 arg3)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            return builder.UseFunc<AppFunc>(app => middleware(app, arg1, arg2, arg3));
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public static IAppBuilder UseFunc<T1, T2, T3, T4>(this IAppBuilder builder, Func<AppFunc, T1, T2, T3, T4, AppFunc> middleware, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            return builder.UseFunc<AppFunc>(app => middleware(app, arg1, arg2, arg3, arg4));
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public static IAppBuilder UseFunc<T1>(this IAppBuilder builder, Func<T1, Func<AppFunc, AppFunc>> middleware, T1 arg1)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            return builder.UseFunc<AppFunc>(app => middleware(arg1)(app));
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public static IAppBuilder UseFunc<T1, T2>(this IAppBuilder builder, Func<T1, T2, Func<AppFunc, AppFunc>> middleware, T1 arg1, T2 arg2)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            return builder.UseFunc<AppFunc>(app => middleware(arg1, arg2)(app));
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public static IAppBuilder UseFunc<T1, T2, T3>(this IAppBuilder builder, Func<T1, T2, T3, Func<AppFunc, AppFunc>> middleware, T1 arg1, T2 arg2, T3 arg3)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            return builder.UseFunc<AppFunc>(app => middleware(arg1, arg2, arg3)(app));
        }

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
        public static IAppBuilder UseFilter(
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
        public static IAppBuilder UseFilter(
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
        public static IAppBuilder UseFilter(
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

                    var syncContext = SynchronizationContext.Current;
                    return task.ContinueWith(t =>
                        {
                            if (t.IsFaulted || t.IsCanceled)
                            {
                                return t;
                            }
                            if (syncContext == null)
                            {
                                return next(env);
                            }
                            var tcs = new TaskCompletionSource<Task>();
                            syncContext.Post(state =>
                            {
                                try
                                {
                                    ((TaskCompletionSource<Task>)state).TrySetResult(next(env));
                                }
                                catch (Exception ex)
                                {
                                    ((TaskCompletionSource<Task>)state).TrySetException(ex);
                                }
                            }, tcs);
                            return tcs.Task.Unwrap();
                        }, TaskContinuationOptions.ExecuteSynchronously).Unwrap();
                });
        }
    }
#endregion

#region StartupExtensions.Type

    internal static partial class StartupExtensions
    {
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "By design")]
        public static IAppBuilder UseType<TMiddleware>(this IAppBuilder builder, params object[] args)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            return builder.Use(typeof(TMiddleware), args);
        }

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
