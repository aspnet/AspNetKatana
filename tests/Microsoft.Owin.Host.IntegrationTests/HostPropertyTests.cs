// -----------------------------------------------------------------------
// <copyright file="HostPropertyTests.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
// </copyright>
// -----------------------------------------------------------------------

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
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class HostPropertyTests : TestBase
    {
        public void StartupPropertiesInspection(IAppBuilder app)
        {
            var properties = app.Properties;
            Assert.NotNull(properties);

            CancellationToken ct = properties.Get<CancellationToken>("host.OnAppDisposing");
            Assert.True(ct.CanBeCanceled);
            Assert.False(ct.IsCancellationRequested);

            string appName = properties.Get<string>("host.AppName");
            Assert.NotNull(appName);

            TextWriter trace = properties.Get<TextWriter>("host.TraceOutput");
            Assert.NotNull(trace);

            app.Run(new AppFunc(env =>
            {
                var tcs = new TaskCompletionSource<object>();
                tcs.TrySetResult(null);
                return tcs.Task;
            }));
        }

        public void RuntimePropertiesInspection(IAppBuilder app)
        {
            app.Run(new AppFunc(env =>
            {
                Assert.NotNull(env);

                CancellationToken ct = env.Get<CancellationToken>("host.OnAppDisposing");
                Assert.True(ct.CanBeCanceled);
                Assert.False(ct.IsCancellationRequested);

                string appName = env.Get<string>("host.AppName");
                Assert.NotNull(appName);

                TextWriter trace = env.Get<TextWriter>("host.TraceOutput");
                Assert.NotNull(trace);

                var tcs = new TaskCompletionSource<object>();
                tcs.TrySetResult(null);
                return tcs.Task;
            }));
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task StartupPropertiesInspection_Success(string serverName)
        {
            var port = RunWebServer(
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
            var port = RunWebServer(
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
