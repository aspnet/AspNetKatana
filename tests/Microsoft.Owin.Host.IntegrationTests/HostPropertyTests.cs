// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Owin;
using Shouldly;
using Xunit;
using Xunit.Extensions;

#if NET40
namespace Microsoft.Owin.Host40.IntegrationTests
#else

namespace Microsoft.Owin.Host45.IntegrationTests
#endif
{
    public class HostPropertyTests : TestBase
    {
        public void StartupPropertiesInspection(IAppBuilder app)
        {
            IDictionary<string, object> properties = app.Properties;
            Assert.NotNull(properties);

            var ct = properties.Get<CancellationToken>("host.OnAppDisposing");
            Assert.True(ct.CanBeCanceled);
            Assert.False(ct.IsCancellationRequested);

            var appName = properties.Get<string>("host.AppName");
            Assert.NotNull(appName);

            var trace = properties.Get<TextWriter>("host.TraceOutput");
            Assert.NotNull(trace);

            app.Run(context => { return TaskHelpers.Completed(); });
        }

        public void RuntimePropertiesInspection(IAppBuilder app)
        {
            app.Run(context =>
            {
                Assert.NotNull(context);

                var ct = context.Get<CancellationToken>("host.OnAppDisposing");
                Assert.True(ct.CanBeCanceled);
                Assert.False(ct.IsCancellationRequested);

                var appName = context.Get<string>("host.AppName");
                Assert.NotNull(appName);

                var trace = context.Get<TextWriter>("host.TraceOutput");
                Assert.NotNull(trace);

                return TaskHelpers.Completed();
            });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task StartupPropertiesInspection_Success(string serverName)
        {
            int port = RunWebServer(
                serverName,
                StartupPropertiesInspection);

            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            return client.GetAsync("http://localhost:" + port + "/text")
                         .Then(response =>
                         {
                             response.Content.ReadAsStringAsync().Result.ShouldBe(string.Empty);
                             response.StatusCode.ShouldBe(HttpStatusCode.OK);
                         });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task RuntimePropertiesInspection_Success(string serverName)
        {
            int port = RunWebServer(
                serverName,
                RuntimePropertiesInspection);

            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            return client.GetAsync("http://localhost:" + port + "/text")
                         .Then(response =>
                         {
                             response.Content.ReadAsStringAsync().Result.ShouldBe(string.Empty);
                             response.StatusCode.ShouldBe(HttpStatusCode.OK);
                         });
        }
    }
}
