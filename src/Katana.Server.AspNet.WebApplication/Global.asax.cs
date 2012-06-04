using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Routing;
using Owin;

namespace Katana.Server.AspNet.WebApplication
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            RouteTable.Routes.MapOwinRoute("hello", DefaultApp);
        }

        private void DefaultApp(IDictionary<string, object> env, ResultDelegate result, Action<Exception> fault)
        {
            var req = new Gate.Request(env);
            result(
                "200 OK",
                new Dictionary<string, IEnumerable<string>>(StringComparer.OrdinalIgnoreCase)
                    {
                        {"Content-Type", new[] {"text/plain"}}
                    },
                (write, flush, end, cancel) =>
                {
                    Action<string> writeText = text => write(new ArraySegment<byte>(Encoding.UTF8.GetBytes(text)));

                    writeText("Hello world\r\n");
                    writeText("PathBase: " + req.PathBase + "\r\n");
                    writeText("Path: " + req.Path + "\r\n");
                    end(null);
                });
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}