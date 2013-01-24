// <copyright file="ExceptionsTests.cs" company="Katana contributors">
//   Copyright 2011-2013 Katana contributors
// </copyright>
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

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Owin;
using Shouldly;
using Xunit.Extensions;

#if NET40
namespace Microsoft.Owin.Host40.IntegrationTests
#else

namespace Microsoft.Owin.Host45.IntegrationTests
#endif
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class ExceptionsTests : TestBase
    {
        public void UnhandledSyncException(IAppBuilder app)
        {
            app.Run(new AppFunc(env => { throw new Exception(); }));
        }

        public void UnhandledAsyncException(IAppBuilder app)
        {
            app.Run(new AppFunc(env =>
            {
                var tcs = new TaskCompletionSource<object>();
                tcs.TrySetException(new Exception());
                return tcs.Task;
            }));
        }

        public void OnSendingHeadersException(IAppBuilder app)
        {
            app.Run(new AppFunc(env =>
            {
                Action<Action<object>, object> onSendingHeaders = (Action<Action<object>, object>)env["server.OnSendingHeaders"];
                onSendingHeaders(_ => { throw new Exception(); }, null);
                var tcs = new TaskCompletionSource<object>();
                tcs.TrySetResult(null);
                return tcs.Task;
            }));
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task SyncException_StatusCode500Expected(string serverName)
        {
            int port = RunWebServer(
                serverName,
                UnhandledSyncException);

            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            return client.GetAsync("http://localhost:" + port + "/text")
                .Then(response => response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError));
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task AsyncException_StatusCode500Expected(string serverName)
        {
            int port = RunWebServer(
                serverName,
                UnhandledAsyncException);

            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            return client.GetAsync("http://localhost:" + port + "/text")
                .Then(response => response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError));
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task OnSendingHeadersException_StatusCode500Expected(string serverName)
        {
            int port = RunWebServer(
                serverName,
                OnSendingHeadersException);

            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            return client.GetAsync("http://localhost:" + port + "/text")
                .Then(response => response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError));
        }
    }
}
