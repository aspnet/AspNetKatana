// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using FunctionalTests.Common;
using Microsoft.Owin;
using Owin;
using Xunit;
using Xunit.Extensions;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace FunctionalTests.Facts.MiddlewarePatterns
{
    public class InstanceBasedMiddleware
    {
        [Theory, Trait("FunctionalTests", "General")]
        [InlineData(HostType.HttpListener)]
        [InlineData(HostType.IIS)]
        public void InstanceBasedMiddlewareTest(HostType hostType)
        {
            using (ApplicationDeployer deployer = new ApplicationDeployer())
            {
                string applicationUrl = deployer.Deploy(hostType, Configuration);
                Assert.Equal("SUCCESS", HttpClientUtility.GetResponseTextFromUrl(applicationUrl));
            }
        }

        public void Configuration(IAppBuilder app)
        {
            app.Use(new MyInstanceMiddleware(), "SUCCESS");
        }
    }

    public class MyInstanceMiddleware
    {
        private AppFunc _next;
        private string _breadcrumb;

        public MyInstanceMiddleware()
        {
        }

        public void Initialize(AppFunc next, string breadcrumb)
        {
            _next = next;
            _breadcrumb = breadcrumb;
        }

        public Task Invoke(IDictionary<string, object> environment)
        {
            var response = new OwinResponse(environment);
            return response.WriteAsync(this._breadcrumb);
        }
    }
}
