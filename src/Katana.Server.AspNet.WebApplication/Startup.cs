using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Owin;
using Gate;
using Gate.Middleware;
using Katana.WebApi;

namespace Katana.Server.AspNet.WebApplication
{
    public class Startup
    {
        public void Configuration(IAppBuilder builder)
        {
            builder
                .UseShowExceptions()
                .UseMessageHandler<TraceRequestFilter>()
                .Run(Wilson.App());
        }
    }
}