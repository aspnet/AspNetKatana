// <copyright file="AdaptersTests.cs" company="Katana contributors">
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
using System.IO;
using System.Linq;
using System.Net.Http;
using Shouldly;
using Xunit;

namespace Microsoft.AspNet.WebApi.Owin.Tests
{
    public class AdaptersTests
    {
        private IDictionary<string, object> NewEnvironment(Action<IDictionary<string, object>> setupEnv, Action<IDictionary<string, string[]>> setupHeaders)
        {
            var headers = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            var env = new Dictionary<string, object>
            {
                { "owin.RequestMethod", "POST" },
                { "owin.RequestScheme", "http" },
                { "owin.RequestPathBase", string.Empty },
                { "owin.RequestPath", "/" },
                { "owin.RequestQueryString", string.Empty },
                { "owin.RequestHeaders", headers },
                { "owin.RequestBody", new MemoryStream() },
            };
            setupEnv(env);
            setupHeaders(headers);
            return env;
        }

        [Fact]
        public void OwinCallWillBecomeRequestMessage()
        {
            HttpRequestMessage requestMessage = OwinHttpMessageUtils.GetRequestMessage(NewEnvironment(x => { }, x => { }));
            requestMessage.Method.Method.ShouldBe("POST");
        }

        [Fact]
        public void CallParamatersBecomeRequestUri()
        {
            IDictionary<string, object> call1 = NewEnvironment(
                x => x
                    .Set("owin.RequestPathBase", "/hello")
                    .Set("owin.RequestPath", "/world")
                    .Set("owin.RequestQueryString", "alpha=1&beta=2"),
                x => x
                    .Set("Host", "gamma.com:1234"));

            HttpRequestMessage message1 = OwinHttpMessageUtils.GetRequestMessage(call1);
            message1.RequestUri.AbsoluteUri.ShouldBe("http://gamma.com:1234/hello/world?alpha=1&beta=2");

            IDictionary<string, object> call2 = NewEnvironment(
                x => x
                    .Set("owin.RequestScheme", "https")
                    .Set("owin.RequestPathBase", string.Empty)
                    .Set("owin.RequestPath", "/one/two")
                    .Set("owin.RequestQueryString", string.Empty),
                x => x
                    .Set("Host", "delta.com"));

            HttpRequestMessage message2 = OwinHttpMessageUtils.GetRequestMessage(call2);
            message2.RequestUri.AbsoluteUri.ShouldBe("https://delta.com/one/two");
        }

        [Fact]
        public void CallHeadersBecomeMessageAndContentHeaders()
        {
            IDictionary<string, object> call = NewEnvironment(
                x => { },
                x => x
                    .Set("Host", "testing")
                    .Set("User-Agent", "Alpha")
                    .Set("Content-Type", "text/plain"));

            HttpRequestMessage message = OwinHttpMessageUtils.GetRequestMessage(call);
            message.Headers.UserAgent.Single().Product.Name.ShouldBe("Alpha");
            message.Content.Headers.ContentType.MediaType.ShouldBe("text/plain");
        }
    }
}
