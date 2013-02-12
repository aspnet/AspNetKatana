// <copyright file="Startup.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
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
// </copyright>

using System;
using System.Web.Http;
using System.Web.Http.SelfHost;
using Microsoft.Owin.Mapping;
using Owin;

namespace Katana.Performance.ReferenceApp
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // app.Use(typeof(AutoTuneMiddleware), app.Properties["Microsoft.Owin.Host.HttpListener.OwinHttpListener"]);
            app.UseSendFileFallback();
            app.UseType<CanonicalRequestPatterns>();

            app.UseFileServer(opt => opt.WithPhysicalPath("Public"));

            app.MapPath("/static-compression", map => map
                .UseStaticCompression()
                .UseFileServer(opt =>
                {
                    opt.WithDirectoryBrowsing();
                    opt.WithPhysicalPath("Public");
                }));

            app.MapPath("/danger", map => map
                .UseStaticCompression()
                .UseFileServer(opt =>
                {
                    opt.WithDirectoryBrowsing();
                    opt.StaticFileOptions.ServeUnknownFileTypes = true;
                }));

            // WebApi
            var config = new HttpSelfHostConfiguration("http://localhost:8080");
            config.Routes.MapHttpRoute("Default", "api/{controller}/{customerID}", new { controller = "Customer", customerID = RouteParameter.Optional });
            config.Formatters.XmlFormatter.UseXmlSerializer = true;
            config.Formatters.JsonFormatter.UseDataContractJsonSerializer = true;
            app.UseHttpServer(config);
        }
    }
}
