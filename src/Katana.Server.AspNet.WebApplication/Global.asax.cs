using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Routing;
using Gate.Middleware;
using Owin;
using Gate;

namespace Katana.Server.AspNet.WebApplication
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            RouteTable.Routes.MapOwinRoute("crash", builder => builder.UseShowExceptions().RunDirect(SampleTwo));
            RouteTable.Routes.MapOwinRoute(DefaultApp);
        }

        private void SampleTwo(Request req, Response res)
        {
            res.ContentType = "text/plain";
            res.Start(
                () =>
                {
                    res.Write("Hello World\r\n");
                    res.Write("PathBase: {0}\r\n", req.PathBase);
                    res.Write("Path: {0}\r\n", req.Path);
                    throw new Exception("Boom");
                    res.End();
                });
        }

        private void DefaultApp(IDictionary<string, object> env, ResultDelegate result, Action<Exception> fault)
        {
            var req = new Request(env);
            result(
                "200 OK",
                new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
                    {
                        {"Content-Type", new[] {"text/plain"}}
                    },
                (write, end, cancel) =>
                {
                    Action<string> writeText = text => write(new ArraySegment<byte>(Encoding.UTF8.GetBytes(text)), null);

                    writeText("Hello world\r\n");
                    writeText("PathBase: " + req.PathBase + "\r\n");
                    writeText("Path: " + req.Path + "\r\n");
                    end(null);
                });
        }
    }
}