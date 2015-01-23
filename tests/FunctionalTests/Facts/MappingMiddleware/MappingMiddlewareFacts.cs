// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
                Assert.Equal(HttpClientUtility.GetResponseTextFromUrl(applicationUrl + "anonymous1"), "Anonymous1");
                Assert.Equal(HttpClientUtility.GetResponseTextFromUrl(applicationUrl + "anonymous2"), "Anonymous2");
                Assert.Equal(HttpClientUtility.GetResponseTextFromUrl(applicationUrl + "a"), "/a");
                Assert.Equal(HttpClientUtility.GetResponseTextFromUrl(applicationUrl + "a/b"), "/a/b");

                //Default application
                Assert.Equal(HttpClientUtility.GetResponseTextFromUrl(applicationUrl), "Default");
            }
        }

        public void MappingMiddlewareConfiguration(IAppBuilder app)
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