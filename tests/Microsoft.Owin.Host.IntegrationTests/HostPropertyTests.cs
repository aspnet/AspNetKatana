// <copyright file="HostPropertyTests.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

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
            IDictionary<string, object> properties = app.Properties;
            Assert.NotNull(properties);

            var ct = properties.Get<CancellationToken>("host.OnAppDisposing");
            Assert.True(ct.CanBeCanceled);
            Assert.False(ct.IsCancellationRequested);

            var appName = properties.Get<string>("host.AppName");
            Assert.NotNull(appName);

            var trace = properties.Get<TextWriter>("host.TraceOutput");
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

                var ct = env.Get<CancellationToken>("host.OnAppDisposing");
                Assert.True(ct.CanBeCanceled);
                Assert.False(ct.IsCancellationRequested);

                var appName = env.Get<string>("host.AppName");
                Assert.NotNull(appName);

                var trace = env.Get<TextWriter>("host.TraceOutput");
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
