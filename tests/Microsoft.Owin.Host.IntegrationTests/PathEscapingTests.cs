// <copyright file="PathEscapingTests.cs" company="Katana contributors">
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
using System.IO;
using System.Net;
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

    public class PathEscapingTests : TestBase
    {
        public void EchoPath(IAppBuilder app)
        {
            app.Run(new AppFunc(env =>
            {
                string path = (string)env["owin.RequestPath"];
                StreamWriter writer = new StreamWriter((Stream)env["owin.ResponseBody"]);
                writer.Write(path);
                writer.Flush();
                return TaskHelpers.Completed();
            }));
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task VerifyUnescapedBackslash_Success(string serverName)
        {
            int port = RunWebServer(
                serverName,
                EchoPath);

            var client = new HttpClient();
            return client.GetAsync("http://localhost:" + port + "/extra%5Cslash/")
                .Then(response =>
                {
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    Assert.Equal("/extra\\slash/", response.Content.ReadAsStringAsync().Result);
                });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        public Task VerifyUnescapedBackslashConverted_Success(string serverName)
        {
            int port = RunWebServer(
                serverName,
                EchoPath);

            var client = new HttpClient();
            return client.GetAsync("http://localhost:" + port + "/extra%5Cslash/")
                .Then(response =>
                {
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    Assert.Equal("/extra/slash/", response.Content.ReadAsStringAsync().Result);
                });
        }

        [Theory]
        [InlineData("Microsoft.Owin.Host.SystemWeb")]
        [InlineData("Microsoft.Owin.Host.HttpListener")]
        public Task VerifyUnescapedCharacters_Success(string serverName)
        {
            string expected = "/"
                + " !\"#$'(),-./"
                + "0123456789"
                + ";=@"
                + "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
                + "[]^_`"
                + "abcdefghijklmnopqrstuvwxyz"
                + "{|}~";

            int port = RunWebServer(
                serverName,
                EchoPath);

            var client = new HttpClient();
            // Error code comments refer to IIS/Asp.Net restrictions.
            return client.GetAsync("http://localhost:" + port + "/"
                // + "%01%02%03%04%05%06%07%08%09%0A%0B%0C%0D%0E%0F%10%11%12%13%14%15%16%17%18%19%1A%1B%1C%1D%1E%1F" // 400
                + "%20%21%22%23%24" // SP!"#$
                // + "%25" // % 404 
                // + "%26" // & 400
                + "%27%28%29" // '()
                // + "%2A" // * 400
                // + "%2B" // + 404
                + "%2C%2D%2E%2F" // ,.-/
                + "%30%31%32%33%34%35%36%37%38%39" // 0-9
                // + "%3A" // : 400
                + "%3B" // ;
                // + "%3C" // < 400
                + "%3D" // =
                // + "%3E%3F" // >? 400
                + "%40" // @
                + "%41%42%43%44%45%46%47%48%49%4A%4B%4C%4D%4E%4F%50%51%52%53%54%55%56%57%58%59%5A" // A-Z
                + "%5B" // [
                // + "%5C" // \ Asp.Net changes this to /
                + "%5D%5E%5F%60" // ]^_`
                + "%61%62%63%64%65%66%67%68%69%6A%6B%6C%6D%6E%6F%70%71%72%73%74%75%76%77%78%79%7A" // a-z
                + "%7B%7C%7D%7E" /* {|}~  "%7F" 400 */)
                    .Then(response =>
                    {
                        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                        Assert.Equal(expected, response.Content.ReadAsStringAsync().Result);
                    });
        }
    }
}
