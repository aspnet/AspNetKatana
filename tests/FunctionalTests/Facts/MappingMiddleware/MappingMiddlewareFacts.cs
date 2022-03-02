// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using FunctionalTests.Common;
using Microsoft.Owin;
using Owin;
using Xunit;
using Xunit.Extensions;

namespace FunctionalTests.Facts.MappingMiddleware
{
    public class MappingMiddlewareFacts
    {
        [Theory, Trait("FunctionalTests", "General")]
        [InlineData(HostType.HttpListener)]
        [InlineData(HostType.IIS)]
        public void MappingMiddleware(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                string applicationUrl = deployer.Deploy(hostType, MappingMiddlewareConfiguration);

                //Anonymous Auth routes test
                Assert.Equal("Anonymous1", HttpClientUtility.GetResponseTextFromUrl(applicationUrl + "anonymous1"));
                Assert.Equal("Anonymous2", HttpClientUtility.GetResponseTextFromUrl(applicationUrl + "anonymous2"));
                Assert.Equal("/a", HttpClientUtility.GetResponseTextFromUrl(applicationUrl + "a"));
                Assert.Equal("/a/b", HttpClientUtility.GetResponseTextFromUrl(applicationUrl + "a/b"));

                //Default application
                Assert.Equal("Default", HttpClientUtility.GetResponseTextFromUrl(applicationUrl));
            }
        }

        internal void MappingMiddlewareConfiguration(IAppBuilder app)
        {
            app.MapWhen(context => { return context.Request.Path.Value.Contains("/anonymous1"); }, appBuilder =>
            {
                appBuilder.Use<MyApplication>("Anonymous1");
            });

            app.MapWhenAsync(context => { return Task.Run<bool>(() => context.Request.Path.Value.Contains("/anonymous2")); }, appBuilder =>
            {
                appBuilder.Use<MyApplication>("Anonymous2");
            });

            app.Map("/a/b", appBuilder => { appBuilder.Use<MyApplication>("/a/b"); });
            app.Map("/a", appBuilder => { appBuilder.Use<MyApplication>("/a"); });
            app.Use<MyApplication>("Default");
        }

        public class MyApplication : OwinMiddleware
        {
            private string applicationName;

            public MyApplication(OwinMiddleware next, string applicationName)
                : base(next)
            {
                this.applicationName = applicationName;
            }

            public override Task Invoke(IOwinContext context)
            {
                return context.Response.WriteAsync(applicationName);
            }
        }
    }

    //[RoutePrefix("Mapping")]
    //public class MappingController : ApiController
    //{
    //    [Route()]
    //    [HttpGet]
    //    public string Get()
    //    {
    //        return "SUCCESS";
    //    }
    //}
}