//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Katana Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Gate.Middleware;
using Owin;

namespace Katana.Sample.HelloWorld
{
    public class Startup
    {
        public void Configuration(IAppBuilder builder)
        {
            var traceOutput = builder.Properties.Get<TextWriter>("host.TraceOutput");
            var addresses = builder.Properties.Get<IList<IDictionary<string, object>>>("host.Addresses");
            var appName = builder.Properties.Get<string>("host.AppName");
            var onAppDisposing = builder.Properties.Get<Action<Action>>("host.OnAppDisposing");

            traceOutput.WriteLine("Starting {0}", appName);

            addresses.Add(new Dictionary<string, object>
            {
                { "scheme", "http" },
                { "host", "+" },
                { "port", "8081" },
                { "path", "/hello" },
            });

            builder
                .UseShowExceptions()
                .Run(Wilson.App());

            onAppDisposing(
                () =>
                {
                    traceOutput.WriteLine("Stopping {0}", appName);
                    traceOutput.Flush();
                    Thread.Sleep(TimeSpan.FromSeconds(2.5));
                });
        }
    }
}
