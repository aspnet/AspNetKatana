using System;
using Gate.Middleware;
using Owin;
using System.Threading.Tasks;
using System.Collections.Generic;
using Gate;
using System.IO;

namespace Katana.Sample.HelloMono
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class Startup
    {
        public void Configuration(IAppBuilder builder)
        {
            var output = (TextWriter)builder.Properties["host.TraceOutput"];
            output.WriteLine("Starting");

            builder.UseFunc(ReplacePath("/", "/Index.html"));
            builder.UseShowExceptions();
            builder.Map("/wilson", new Wilson());
            builder.Map("/hello", this);
        }

        public Func<AppFunc, AppFunc> ReplacePath(string match, string replacement)
        {
            return next => env =>
            {
                var req = new Request(env);
                if (req.Path == match)
                {
                    req.Path = replacement;
                }
                return next(env);
            };
        }

        public Task Invoke(IDictionary<string, object> env)
        {
            var req = new Request(env);
            req.TraceOutput.WriteLine("Request {0} at {1}{2}", req.Method, req.PathBase, req.Path);

            var resp = new Response(env) { ContentType = "text/plain" };
            return resp.WriteAsync("Hello, mono");
        }
    }
}

