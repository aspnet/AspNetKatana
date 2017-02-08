// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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

namespace Microsoft.Owin.Host.IntegrationTests
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

            app.Run(context => { return Task.FromResult(0); });
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

                return Task.FromResult(0);
            });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public async Task StartupPropertiesInspection_Success(string serverName)
        {
            int port = RunWebServer(
                serverName,
                StartupPropertiesInspection);

            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            var response = await client.GetAsync("http://localhost:" + port + "/text");
            response.Content.ReadAsStringAsync().Result.ShouldBe(string.Empty);
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public async Task RuntimePropertiesInspection_Success(string serverName)
        {
            int port = RunWebServer(
                serverName,
                RuntimePropertiesInspection);

            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            var response = await client.GetAsync("http://localhost:" + port + "/text");
            response.Content.ReadAsStringAsync().Result.ShouldBe(string.Empty);
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }
    }
}
