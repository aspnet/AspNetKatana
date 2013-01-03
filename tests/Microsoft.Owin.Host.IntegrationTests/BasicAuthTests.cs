// <copyright file="BasicAuthTests.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
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
using System.Net.Http.Headers;
using System.Security.Principal;
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

    public class BasicAuthTests : TestBase
    {
        public void InvalidCredentials(IAppBuilder app)
        {
            app.UseBasicAuth((a, b) => CompletedTask(false));
            app.Run(new AppFunc(env =>
            {
                throw new NotImplementedException();
            }));
        }

        public void ValidCredentials(IAppBuilder app)
        {
            app.UseBasicAuth((a, b) => CompletedTask(true));
            app.Run(new AppFunc(env =>
            {
                if (env["server.User"] == null
                    || env["server.User"].GetType() != typeof(GenericPrincipal))
                {
                    throw new InvalidOperationException();
                }
                env["owin.ResponseStatusCode"] = 201;
                return CompletedTask(true);
            }));
        }

        public void AnonymousCredentials(IAppBuilder app)
        {
            app.UseBasicAuth((a, b) => CompletedTask(false));
            app.Run(new AppFunc(env =>
            {
                object user = env["server.User"];

                if (user == null)
                {
                    env["owin.ResponseStatusCode"] = 201;
                    return CompletedTask(true);
                }
                if (user.GetType() == typeof(WindowsPrincipal))
                {
                    WindowsIdentity identity = ((WindowsPrincipal)user).Identity as WindowsIdentity;
                    if (identity.IsAnonymous)
                    {
                        env["owin.ResponseStatusCode"] = 201;
                        return CompletedTask(true);
                    }
                }

                throw new InvalidOperationException();
            }));
        }

        public void DenyAnonymousCredentials(IAppBuilder app)
        {
            app.UseBasicAuth((a, b) => CompletedTask(false));
            app.UseDenyAnonymous();
            app.Run(new AppFunc(env =>
            {
                throw new NotImplementedException();
            }));
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task AccessDenied_401(string serverName)
        {
            var port = RunWebServer(
                serverName,
                InvalidCredentials);

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("basic", "dXNlcjpwc3dk");
            client.Timeout = TimeSpan.FromSeconds(5);
            return client.GetAsync("http://localhost:" + port + "/text")
                .Then(response => response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized));
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task AccessAccepted_201(string serverName)
        {
            var port = RunWebServer(
                serverName,
                ValidCredentials);

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("basic", "dXNlcjpwc3dk");
            client.Timeout = TimeSpan.FromSeconds(5);
            return client.GetAsync("http://localhost:" + port + "/text")
                .Then(response => response.StatusCode.ShouldBe((HttpStatusCode)201));
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task Anonymous_201(string serverName)
        {
            var port = RunWebServer(
                serverName,
                AnonymousCredentials);

            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            return client.GetAsync("http://localhost:" + port + "/text")
                .Then(response => response.StatusCode.ShouldBe((HttpStatusCode)201));
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task DenyAnonymous_401(string serverName)
        {
            var port = RunWebServer(
                serverName,
                DenyAnonymousCredentials);

            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            return client.GetAsync("http://localhost:" + port + "/text")
                .Then(response => response.StatusCode.ShouldBe((HttpStatusCode)401));
        }

        private static Task<bool> CompletedTask(bool result)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            tcs.TrySetResult(result);
            return tcs.Task;
        }
    }
}
