// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FunctionalTests.Common;
using Microsoft.Owin;
using Owin;
using Xunit;
using Xunit.Extensions;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace FunctionalTests.Facts.InstanceContext
{
    public class InstanceContextTest
    {
        [Theory, Trait("FunctionalTests", "General")]
        [InlineData(HostType.HttpListener)]
        [InlineData(HostType.IIS)]
        public void InstanceContext(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                string applicationUrl = deployer.Deploy<InstanceContextStartup>(hostType);
                string[] urls = new string[] { applicationUrl, applicationUrl + "/one", applicationUrl + "/two" };

                bool failed = false;

                foreach (string url in urls)
                {
                    string previousResponse = null;
                    for (int count = 0; count < 3; count++)
                    {
                        string currentResponse = HttpClientUtility.GetResponseTextFromUrl(url);

                        if (!currentResponse.Contains("SUCCESS") || (previousResponse != null && currentResponse != previousResponse))
                        {
                            failed = true;
                        }

                        previousResponse = currentResponse;
                    }
                }

                Assert.True(!failed, "At least one of the instance contexts is not correct");
            }
        }
    }

    public class InstanceContextStartup
    {
        Dictionary<string, AppFunc> RoutingTable = new Dictionary<string, AppFunc>();

        public void Configuration(IAppBuilder app)
        {
            SetupRouteTable(app);

            // Build the application
            app.Use((context, next) =>
            {
                if (context.Request.Path.Value.StartsWith("/one", StringComparison.OrdinalIgnoreCase))
                {
                    return RoutingTable["/one"](context.Environment);
                }
                else if (context.Request.Path.Value.StartsWith("/two", StringComparison.OrdinalIgnoreCase))
                {
                    return RoutingTable["/two"](context.Environment);
                }
                else
                {
                    return next();
                }
            });

            app.Use(typeof(DefaultApplication));
        }

        public void SetupRouteTable(IAppBuilder app)
        {
            // Initiate the route table
            RoutingTable.Add("/one", (AppFunc)app.New().Use(typeof(DefaultMiddleWare)).Use(typeof(DefaultApplication)).Build(typeof(AppFunc)));
            RoutingTable.Add("/two", (AppFunc)app.New().Use(typeof(DefaultMiddleWare)).Use(typeof(DefaultApplication)).Build(typeof(AppFunc)));
        }
    }

    public class DefaultMiddleWare : OwinMiddleware
    {
        public DefaultMiddleWare(OwinMiddleware next)
            : base(next)
        {
        }

        public override Task Invoke(IOwinContext context)
        {
            context.Set<string>("hashcode", this.GetHashCode().ToString());
            return this.Next.Invoke(context);
        }
    }

    public class DefaultApplication : OwinMiddleware
    {
        public DefaultApplication(OwinMiddleware next)
            : base(next)
        {
        }

        public override Task Invoke(IOwinContext context)
        {
            var responseText = context.Get<string>("hashcode") != null ?
                "SUCCESS" + context.Get<string>("hashcode") + "," + this.GetHashCode() :
                "SUCCESS" + this.GetHashCode().ToString();

            return context.Response.WriteAsync(responseText);
        }
    }
}
