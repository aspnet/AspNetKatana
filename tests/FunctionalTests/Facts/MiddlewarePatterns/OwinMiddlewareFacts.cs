// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using FunctionalTests.Common;
using Microsoft.Owin;
using Owin;
using Xunit;
using Xunit.Extensions;

namespace FunctionalTests.Facts.MiddlewarePatterns
{
    public class OwinMiddlewareFacts
    {
        [Theory, Trait("FunctionalTests", "General")]
        [InlineData(HostType.HttpListener)]
        [InlineData(HostType.IIS)]
        public void OwinAbstractMiddleware(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                string applicationUrl = deployer.Deploy(hostType, OwinAbstractMiddlewareConfiguration);
                Assert.Equal("SUCCESS", HttpClientUtility.GetResponseTextFromUrl(applicationUrl));
            }
        }

        public void OwinAbstractMiddlewareConfiguration(IAppBuilder app)
        {
            app.Use<PassThroughMiddleware>("p1", 2, "p3");
            app.Use(typeof(MyApplication));
        }
    }

    class PassThroughMiddleware : OwinMiddleware
    {
        string p1;
        int p2;
        object p3;

        public PassThroughMiddleware(OwinMiddleware next, string p1, int p2, object p3)
            : base(next)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
        }

        public override Task Invoke(IOwinContext context)
        {
            context.Set<string>("p1", p1);
            context.Set<int>("p2", p2);
            context.Set<object>("p3", p3);
            return this.Next.Invoke(context);
        }
    }

    class MyApplication : OwinMiddleware
    {
        public MyApplication(OwinMiddleware next)
            : base(next)
        {
        }

        public override Task Invoke(IOwinContext context)
        {
            if (context.Get<string>("p1") == "p1" && context.Get<int>("p2") == 2 && context.Get<object>("p3").ToString() == "p3")
            {
                return context.Response.WriteAsync("SUCCESS");
            }
            else
            {
                return context.Response.WriteAsync("FAILURE");
            }
        }
    }
}
