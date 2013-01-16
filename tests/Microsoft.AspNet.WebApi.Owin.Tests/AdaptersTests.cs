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
using System.Threading;
using Shouldly;
using Xunit;

namespace Microsoft.AspNet.WebApi.Owin.Tests
{
    public class AdaptersTests
    {
        private IDictionary<string, object> NewEnvironment(Action<IDictionary<string, object>> setupEnv, Action<IDictionary<string, string[]>> setupHeaders)
        {
            var requestHeaders = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            var responseHeaders = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            var env = new Dictionary<string, object>
            {
                { "owin.RequestMethod", "POST" },
                { "owin.RequestScheme", "http" },
                { "owin.RequestPathBase", string.Empty },
                { "owin.RequestPath", "/" },
                { "owin.RequestQueryString", string.Empty },
                { "owin.RequestHeaders", requestHeaders },
                { "owin.RequestBody", new MemoryStream() },
                { "owin.ResponseHeaders", responseHeaders },
                { "owin.ResponseBody", new MemoryStream() },
            };
            setupEnv(env);
            setupHeaders(requestHeaders);
            return env;
        }

        [Fact]
        public void OwinCallWillBecomeRequestMessage()
        {
            HttpRequestMessage requestMessage = OwinHttpMessageUtilities.GetRequestMessage(NewEnvironment(x => { }, x => { }));
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

            HttpRequestMessage message1 = OwinHttpMessageUtilities.GetRequestMessage(call1);
            message1.RequestUri.AbsoluteUri.ShouldBe("http://gamma.com:1234/hello/world?alpha=1&beta=2");

            IDictionary<string, object> call2 = NewEnvironment(
                x => x
                    .Set("owin.RequestScheme", "https")
                    .Set("owin.RequestPathBase", string.Empty)
                    .Set("owin.RequestPath", "/one/two")
                    .Set("owin.RequestQueryString", string.Empty),
                x => x
                    .Set("Host", "delta.com"));

            HttpRequestMessage message2 = OwinHttpMessageUtilities.GetRequestMessage(call2);
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

            HttpRequestMessage message = OwinHttpMessageUtilities.GetRequestMessage(call);
            message.Headers.UserAgent.Single().Product.Name.ShouldBe("Alpha");
            message.Content.Headers.ContentType.MediaType.ShouldBe("text/plain");
        }

        [Fact]
        public void SendResponseMessageNoContent()
        {
            IDictionary<string, object> call = NewEnvironment(x => { }, x => { });
            OwinHttpMessageUtilities.SendResponseMessage(call, new HttpResponseMessage(), CancellationToken.None).Wait();
            call.Get<int>("owin.ResponseStatusCode").ShouldBe(200);
            call.Get<string>("owin.ResponseReasonPhrase").ShouldBe("OK");
        }

        [Fact]
        public void SendResponseMessageStringContent_ContentLengthSet()
        {
            IDictionary<string, object> call = NewEnvironment(x => { }, x => { });
            OwinHttpMessageUtilities.SendResponseMessage(call,
                new HttpResponseMessage()
                {
                    Content = new StringContent("Hello World")
                }, CancellationToken.None).Wait();
            call.Get<int>("owin.ResponseStatusCode").ShouldBe(200);
            call.Get<string>("owin.ResponseReasonPhrase").ShouldBe("OK");
            var responseHeaders = call.Get<IDictionary<string, string[]>>("owin.ResponseHeaders");
            responseHeaders.Count.ShouldBe(2); // ContentLength, ContentType
            responseHeaders["Content-Type"][0].ShouldBe("text/plain; charset=utf-8");
            responseHeaders["Content-Length"][0].ShouldBe("11");
            call.Get<MemoryStream>("owin.ResponseBody").Length.ShouldBe(11);
        }
    }
}
