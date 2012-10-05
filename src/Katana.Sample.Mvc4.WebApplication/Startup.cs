//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Katana Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

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