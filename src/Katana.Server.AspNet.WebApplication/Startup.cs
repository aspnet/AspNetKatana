// Copyright 2011-2012 Katana contributors
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

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Gate;
using Gate.Middleware;
using Owin;

namespace Katana.Server.AspNet.WebApplication
{
    public class Startup
    {
        public void Configuration(IAppBuilder builder)
        {
            // Run WebApi
            var configuration = new HttpConfiguration(new HttpRouteCollection(HttpRuntime.AppDomainAppVirtualPath));
            configuration.Routes.MapHttpRoute("Default", "{controller}");

            builder.UseShowExceptions();
            builder.UsePassiveValidator();
            builder.UseHttpServer(configuration);

            builder.Map("/wilson", new Wilson());
            builder.Run(this);
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
