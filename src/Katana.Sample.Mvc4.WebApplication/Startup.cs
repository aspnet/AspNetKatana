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

using System.Web;
using System.Web.Http;
using Gate;
using Gate.Middleware;
using Owin;

namespace Katana.Sample.Mvc4.WebApplication
{
    public class Startup
    {
        public void Configuration(IAppBuilder builder)
        {
            var configuration = new HttpConfiguration(new HttpRouteCollection(HttpRuntime.AppDomainAppVirtualPath));
            configuration.Routes.MapHttpRoute("Default", "{controller}");

            builder.UsePassiveValidator();
            builder.UseShowExceptions();
            builder.UseHttpServer(configuration);
            builder.Map("/wilson", new Wilson());
        }
    }
}