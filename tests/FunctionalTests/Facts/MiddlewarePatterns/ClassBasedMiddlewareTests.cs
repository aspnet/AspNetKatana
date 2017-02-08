// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FunctionalTests.Common;
using Owin;
using Xunit;
using Xunit.Extensions;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace FunctionalTests.Facts.MiddlewarePatterns
{
    public class ClassBasedMiddlewareTests
    {
        [Theory, Trait("FunctionalTests", "General")]
        [InlineData(HostType.HttpListener)]
        [InlineData(HostType.IIS)]
        public void ClassBasedMiddlewareTest(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                string applicationUrl = deployer.Deploy<ClassBasedMiddleWareStartup>(hostType);
                Assert.Equal("SUCCESS", HttpClientUtility.GetResponseTextFromUrl(applicationUrl));
            }
        }
    }

    public class ClassBasedMiddleWareStartup
    {
        public void Configuration(IAppBuilder app)
        {
            app.Use<ClassBasedMiddleWare1>();
            app.Use(typeof(ClassBasedMiddleWare2), "string", DateTime.Now, new Dictionary<string, string>());
            app.Run((context) =>
                {
                    if (context.Get<string>("ClassBasedMiddleWare1") != "ClassBasedMiddleWare1" || context.Get<string>("ClassBasedMiddleWare2") != "ClassBasedMiddleWare2")
                    {
                        throw new Exception("Middlewares ClassBasedMiddleWare1 & ClassBasedMiddleWare2 not executed in the pipeline");
                    }

                    return context.Response.WriteAsync("SUCCESS");
                });
        }
    }

    public class ClassBasedMiddleWare1
    {
        private AppFunc next;
        public ClassBasedMiddleWare1(AppFunc next)
        {
            this.next = next;
        }

        public Task Invoke(IDictionary<string, object> env)
        {
            env.Add("ClassBasedMiddleWare1", "ClassBasedMiddleWare1");
            return this.next(env);
        }
    }

    public class ClassBasedMiddleWare2
    {
        private AppFunc next;

        public ClassBasedMiddleWare2(AppFunc next, string strData, DateTime date, IDictionary<string, string> dictionary)
        {
            this.next = next;
        }

        public Task Invoke(IDictionary<string, object> env)
        {
            env.Add("ClassBasedMiddleWare2", "ClassBasedMiddleWare2");
            return this.next(env);
        }
    }
}
