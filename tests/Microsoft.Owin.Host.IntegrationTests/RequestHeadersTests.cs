// <copyright file="RequestHeadersTests.cs" company="Katana contributors">
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

    public class RequestHeadersTests : TestBase
    {
        private const int ExpectedStatusCode = 201;

        public void SetCustomRequestHeader(IAppBuilder app)
        {
            app.Run(new AppFunc(env =>
            {
                var requestHeaders = (IDictionary<string, string[]>)env["owin.RequestHeaders"];
                requestHeaders["custom"] = new string[] { "custom value" };
                env["owin.ResponseStatusCode"] = ExpectedStatusCode;
                return TaskHelpers.Completed();
            }));
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task SetCustomHeaders_Success(string serverName)
        {
            int port = RunWebServer(
                serverName,
                SetCustomRequestHeader);

            var client = new HttpClient();
            return client.GetAsync("http://localhost:" + port + "/custom")
                .Then(response => response.StatusCode.ShouldBe((HttpStatusCode)ExpectedStatusCode));
        }

        public void SetKnownRequestHeader(IAppBuilder app)
        {
            app.Run(new AppFunc(env =>
            {
                var requestHeaders = (IDictionary<string, string[]>)env["owin.RequestHeaders"];
                requestHeaders["Host"] = new string[] { "custom:9090" };
                env["owin.ResponseStatusCode"] = ExpectedStatusCode;
                return TaskHelpers.Completed();
            }));
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task SetKnownHeaders_Success(string serverName)
        {
            int port = RunWebServer(
                serverName,
                SetKnownRequestHeader);

            var client = new HttpClient();
            return client.GetAsync("http://localhost:" + port + "/known")
                .Then(response => response.StatusCode.ShouldBe((HttpStatusCode)ExpectedStatusCode));
        }

        public void VerifyCaseInsensitivity(IAppBuilder app)
        {
            app.Run(new AppFunc(env =>
            {
                var requestHeaders = (IDictionary<string, string[]>)env["owin.RequestHeaders"];
                requestHeaders["custom"] = new string[] { "custom value" };

                string[] roundTrip = requestHeaders["CuStom"];
                roundTrip.Length.ShouldBe(1);
                roundTrip[0].ShouldBe("custom value");

                env["owin.ResponseStatusCode"] = ExpectedStatusCode;
                return TaskHelpers.Completed();
            }));
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task VerifyCaseInsensitivity_Success(string serverName)
        {
            int port = RunWebServer(
                serverName,
                VerifyCaseInsensitivity);

            var client = new HttpClient();
            return client.GetAsync("http://localhost:" + port + "/case")
                .Then(response => response.StatusCode.ShouldBe((HttpStatusCode)ExpectedStatusCode));
        }
    }
}
