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
using Microsoft.WebSockets.Owin.Samples;

namespace Katana.Server.AspNet.WebSocketsApp
{
    public class Startup
    {
        public void Configuration(IAppBuilder builder)
        {
            builder.UseShowExceptions();
            builder.Use(typeof(WebSocketEcho));
        }
    }
}