// <copyright file="Startup.cs" company="Katana contributors">
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
