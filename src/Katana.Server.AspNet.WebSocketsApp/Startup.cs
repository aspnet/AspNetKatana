//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Katana Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Gate;
using Gate.Middleware;
using Microsoft.WebSockets.Owin.Samples;
using Owin;

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