// <copyright file="SimpleGetTests.cs" company="Microsoft Open Technologies, Inc.">
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
    public class SimpleGetTests : TestBase
    {
        public void TextHtmlAlpha(IAppBuilder app)
        {
            app.Run(context =>
            {
                context.Response.ContentType = "text/html";
                return context.Response.WriteAsync("<p>alpha</p>");
            });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task ResponseBodyShouldArrive(string serverName)
        {
            int port = RunWebServer(
                serverName,
                TextHtmlAlpha);

            var client = new HttpClient();
            return client.GetStringAsync("http://localhost:" + port + "/text")
                .Then(body => body.ShouldBe("<p>alpha</p>"));
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task ContentTypeShouldBeSet(string serverName)
        {
            int port = RunWebServer(
                serverName,
                TextHtmlAlpha);

            var client = new HttpClient();
            return client.GetAsync("http://localhost:" + port + "/text")
                .Then(message => message.Content.Headers.ContentType.MediaType.ShouldBe("text/html"));
        }
    }
}
