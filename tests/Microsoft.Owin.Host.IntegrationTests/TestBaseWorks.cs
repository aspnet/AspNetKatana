// <copyright file="TestBaseWorks.cs" company="Microsoft Open Technologies, Inc.">
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

using System.IO;
using System.Net.Http;
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
    public class TestBaseWorks : TestBase
    {
        [Fact]
        public Task TestShouldRunWebServer()
        {
            int port = RunWebServer(
                serverName: "Microsoft.Owin.Host.SystemWeb",
                application: HelloWorld);

            var client = new HttpClient();

            return client.GetStringAsync("http://localhost:" + port)
                .Then(responseMessage => responseMessage.ShouldBe("Hello world!"));
        }

        public void HelloWorld(IAppBuilder app)
        {
            app.UseFunc(_ => env =>
            {
                var output = (Stream)env["owin.ResponseBody"];
                using (var writer = new StreamWriter(output))
                {
                    writer.Write("Hello world!");
                }
                return TaskHelpers.Completed();
            });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task ServerMayBeSystemWebOrHttpListener(string serverName)
        {
            int port = RunWebServer(
                serverName: serverName,
                application: HelloWorld);

            var client = new HttpClient();

            return client.GetStringAsync("http://localhost:" + port)
                .Then(responseMessage => responseMessage.ShouldBe("Hello world!"));
        }
    }
}
