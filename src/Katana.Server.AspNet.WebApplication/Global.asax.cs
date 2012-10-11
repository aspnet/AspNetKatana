// <copyright file="Global.asax.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
// </copyright>
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gate;
using Owin;

namespace Katana.Server.AspNet.WebApplication
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            // var configuration = new HttpConfiguration(new HttpRouteCollection(HttpRuntime.AppDomainAppVirtualPath));
            // configuration.Routes.MapHttpRoute("Default", "{controller}");

            // RouteTable.Routes.MapOwinRoute("/", builder =>
            // {
            //    builder.UseWebApi(configuration);
            //    builder.Run(this);
            // });

            // RouteTable.Routes.MapOwinRoute("/crash", builder => builder
            //    .UseShowExceptions()
            //    .UseMessageHandler(new TraceRequestFilter())
            //    .UseDirect(SampleTwo));

            // RouteTable.Routes.MapOwinRoute("/show", builder => builder
            //    .UseDirect(Show));

            // RouteTable.Routes.MapOwinRoute("/wilson", builder => builder
            //    .UseShowExceptions()
            //    .UseMessageHandler<TraceRequestFilter>()
            //    .Run(Wilson.App()));

            // RouteTable.Routes.MapOwinRoute("/wilson2", builder => builder
            //    .UseShowExceptions()
            //    .UseMessageHandler(new TraceRequestFilter())
            //    .Run(Wilson.App()));

            // RouteTable.Routes.MapOwinRoute("/auth", builder => builder
            //    .UseShowExceptions()
            //    .UseMessageHandler<TraceRequestFilter>()
            //    .UseMessageHandler<AuthorizeRoleFilter>("hello")
            //    .Run(Wilson.App()));

            // RouteTable.Routes.MapOwinRoute("/auth2", builder => builder
            //    .UseShowExceptions()
            //    .UseMessageHandler(inner => new TraceRequestFilter(inner))
            //    .UseMessageHandler(inner => new AuthorizeRoleFilter(inner, "hello"))
            //    .Run(Wilson.App()));

            // RouteTable.Routes.MapOwinRoute(
            //    "/auth3",
            //    builder =>
            //    {
            //        builder.UseShowExceptions();
            //        builder.UseMessageHandler(inner => new TraceRequestFilter(inner));
            //        builder.UseMessageHandler(inner => new AuthorizeRoleFilter(inner, "hello"));
            //        builder.Run(Wilson.App());
            //    });

            ////simplest
            // RouteTable.Routes.MapOwinRoute("/auth4", Auth4Pipeline);
        }

        // simplest
        private void Auth4Pipeline(IAppBuilder builder)
        {
            // builder.UseShowExceptions();
            // builder.UseTraceRequestFilter();
            // builder.UseAuthorizeRoleFilter("hello");
            // builder.UseMessageHandler(new TraceRequestFilter());
            // builder.UseMessageHandler(new AuthorizeRoleFilter("hello again"));
            // builder.Run(Wilson.App());
        }

        private Task Show(Request req, Response res)
        {
            // res.ContentType = "text/plain";
            // return res.StartAsync().Then(
            //    resp1 =>
            //    {
            //        res.Write("Hello World\r\n");
            //        res.Write("PathBase: {0}\r\n", req.PathBase);
            //        res.Write("Path: {0}\r\n", req.Path);
            //        res.Write("<ul>");
            //        foreach (var kv in req.Environment)
            //        {
            //            res.Write("<li>");
            //            res.Write(kv.Key);
            //            res.Write("&raquo;");
            //            res.Write(Convert.ToString(kv.Value, CultureInfo.InvariantCulture));
            //            res.Write("</li>");
            //        }
            //        res.Write("</ul>");
            //        res.End();
            //    });
            return TaskHelpers.Completed();
        }

        private Task SampleTwo(Request req, Response res)
        {
            // res.ContentType = "text/html";
            // return res.StartAsync().Then(
            //    () =>
            //    {
            //        res.Write("Hello World\r\n");
            //        res.Write("PathBase: {0}\r\n", req.PathBase);
            //        res.Write("Path: {0}\r\n", req.Path);
            //        res.Write("<ul>");
            //        foreach (var kv in req.Environment)
            //        {
            //            res.Write("<li>");
            //            res.Write(kv.Key);
            //            res.Write("&raquo;");
            //            res.Write(Convert.ToString(kv.Value, CultureInfo.InvariantCulture));
            //            res.Write("</li>");
            //        }
            //        res.Write("</ul>");
            //        throw new Exception("Boom");
            //        //res.End();
            //    });
            return TaskHelpers.Completed();
        }

        public Task Invoke(IDictionary<string, object> env)
        {
            var req = new Request(env);
            var resp = new Response(env);
            resp.ContentType = "text/plain";
            resp.Write("Hello world\r\n");
            resp.OutputStream.Flush();
            resp.Write("PathBase: " + req.PathBase + "\r\n");
            resp.Write("Path: " + req.Path + "\r\n");
            return TaskHelpers.Completed();
        }
    }
}
