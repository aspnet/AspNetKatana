using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Owin;

namespace Gate.Middleware
{
    internal static class Cascade
    {
        public static IAppBuilder RunCascade(this IAppBuilder builder, params AppDelegate[] apps)
        {
            return builder.Run(App, apps);
        }

        public static IAppBuilder RunCascade(this IAppBuilder builder, params Action<IAppBuilder>[] apps)
        {
            return builder.Run(App, apps.Select(builder.Build<AppDelegate>));
        }

        public static IAppBuilder UseCascade(this IAppBuilder builder, params AppDelegate[] apps)
        {
            return builder.Use(Middleware, apps);
        }

        public static IAppBuilder UseCascade(this IAppBuilder builder, params Action<IAppBuilder>[] apps)
        {
            return builder.Use(Middleware, apps.Select(builder.Build<AppDelegate>));
        }


        public static AppDelegate App(IEnumerable<AppDelegate> apps)
        {
            return Middleware(null, apps);
        }
        public static AppDelegate App(AppDelegate app0)
        {
            return Middleware(null, new[] { app0 });
        }
        public static AppDelegate App(AppDelegate app0, AppDelegate app1)
        {
            return Middleware(null, new[] { app0, app1 });
        }
        public static AppDelegate App(AppDelegate app0, AppDelegate app1, AppDelegate app2)
        {
            return Middleware(null, new[] { app0, app1, app2 });
        }

        public static AppDelegate Middleware(AppDelegate app, AppDelegate app0)
        {
            return Middleware(app, new[] { app0 });
        }
        public static AppDelegate Middleware(AppDelegate app, AppDelegate app0, AppDelegate app1)
        {
            return Middleware(app, new[] { app0, app1 });
        }
        public static AppDelegate Middleware(AppDelegate app, AppDelegate app0, AppDelegate app1, AppDelegate app2)
        {
            return Middleware(app, new[] { app0, app1, app2 });
        }

        public static AppDelegate Middleware(AppDelegate app, IEnumerable<AppDelegate> apps)
        {
            // sequence to attempt is {apps[0], apps[n], app}
            // or {apps[0], apps[n]} if app is null
            apps = (apps ?? new AppDelegate[0]).Concat(new[] { app ?? NotFound.Call }).ToArray();

            // the first non-404 result will the the one to take effect
            // any subsequent apps are not called
            return (env, result, fault) =>
            {
                var iter = apps.GetEnumerator();
                iter.MoveNext();

                Action loop = () => { };
                loop = () =>
                {
                    var threadId = Thread.CurrentThread.ManagedThreadId;
                    for (var hot = true; hot; )
                    {
                        hot = false;
                        iter.Current.Invoke(
                            env,
                            (status, headers, body) =>
                            {
                                try
                                {
                                    if (status.StartsWith("404") && iter.MoveNext())
                                    {
                                        // ReSharper disable AccessToModifiedClosure
                                        if (threadId == Thread.CurrentThread.ManagedThreadId)
                                        {
                                            hot = true;
                                        }
                                        else
                                        {
                                            loop();
                                        }
                                        // ReSharper restore AccessToModifiedClosure
                                    }
                                    else
                                    {
                                        result(status, headers, body);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    fault(ex);
                                }
                            },
                            fault);
                    }
                    threadId = 0;
                };

                loop();
            };
        }
    }
}