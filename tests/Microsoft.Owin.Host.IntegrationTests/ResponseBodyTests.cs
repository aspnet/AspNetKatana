// <copyright file="ResponseBodyTests.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Net.Http;
using System.Threading.Tasks;
using Owin;
using Xunit;
using Xunit.Extensions;

#if NET40
namespace Microsoft.Owin.Host40.IntegrationTests
#else
namespace Microsoft.Owin.Host45.IntegrationTests
#endif
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class ResponseBodyTests : TestBase
    {
        public void CloseResponseBodyAndWriteExtra(IAppBuilder app)
        {
            app.UseFunc(DelayedWrite);

            app.Run(new AppFunc(env =>
            {
                var writer = new StreamWriter((Stream)env["owin.ResponseBody"]);
                writer.Write("Response");
                writer.Flush();
                writer.Close();
                return TaskHelpers.Completed();
            }));
        }

        private AppFunc DelayedWrite(AppFunc next)
        {
            return env =>
            {
                return next(env)
                    .Then(() =>
                    {
                        var writer = new StreamWriter((Stream)env["owin.ResponseBody"]);
                        writer.Write("AndExtra");
                        writer.Flush();
                        writer.Close();
                    });
            };
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task CloseResponseBodyAndWriteExtra_CloseIgnored(string serverName)
        {
            int port = RunWebServer(
                serverName,
                CloseResponseBodyAndWriteExtra);

            var client = new HttpClient();
            return client.GetAsync("http://localhost:" + port)
                .Then(response =>
                {
                    response.EnsureSuccessStatusCode();
                    Assert.Equal("ResponseAndExtra", response.Content.ReadAsStringAsync().Result);
                });
        }
    }
}
