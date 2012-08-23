using System;
using System.Threading.Tasks;
using Owin;

namespace Gate
{
    // TODO: Remove
    using AppDelegate = Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

    internal static class AppBuilderInlineExtensions
    {
        public static IAppBuilder MapDirect(this IAppBuilder builder, string path, Func<Request, Response, Task> app)
        {
            return builder.Map(path, map => map.UseDirect(app));
        }

        public static IAppBuilder UseDirect(this IAppBuilder builder, Func<Request, Response, Task> app)
        {
            return builder.UseFunc<AppDelegate>(next => environment =>
            {
                var req = new Request(environment);
                var resp = new Response(environment)
                {
                    Next = () => next(environment)
                };

                app.Invoke(req, resp)
                    .Catch(caught =>
                    {
                        resp.End(caught.Exception);
                        return caught.Handled();
                    });
                return resp.Task;
            });
        }
    }
}
