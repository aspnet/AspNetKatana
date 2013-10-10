// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Owin;

namespace Microsoft.Owin.Testing.Tests
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.Run(context =>
            {
                return context.Response.WriteAsync("Startup.Configration");
            });
        }
    }
}
