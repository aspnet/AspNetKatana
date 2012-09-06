using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Gate.Middleware;
using System.Threading;
using System.Threading.Tasks;
using Gate;
using System.IO;
using System.Globalization;
using Katana.Server.DotNetWebSockets;

namespace Katana.Server.AspNet.WebSocketsApp
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class Startup
    {
        public void Configuration(IAppBuilder builder)
        {
            builder.UseShowExceptions();
            builder.Use(typeof(WebSocketEcho));
        }
    }
}