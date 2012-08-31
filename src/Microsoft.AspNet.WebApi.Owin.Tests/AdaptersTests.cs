using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Microsoft.AspNet.WebApi.Owin.Tests
{
    public class AdaptersTests
    {

        IDictionary<string, object> NewEnvironment(Action<IDictionary<string, object>> setupEnv, Action<IDictionary<string, string[]>> setupHeaders)
        {
            var headers = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            var env = new Dictionary<string, object>
            {
                {"owin.RequestMethod", "POST"},
                {"owin.RequestScheme", "http"},
                {"owin.RequestPathBase", ""},
                {"owin.RequestPath", "/"},
                {"owin.RequestQueryString", ""},       
                {"owin.RequestHeaders", headers},
                {"owin.RequestBody", new MemoryStream()},
            };
            setupEnv(env);
            setupHeaders(headers);
            return env;
        }

        [Fact]
        public void OwinCallWillBecomeRequestMessage()
        {
            var requestMessage = OwinHttpMessageUtils.GetRequestMessage(NewEnvironment(x => { }, x => { }));
            requestMessage.Method.Method.ShouldBe("POST");
        }

        [Fact]
        public void CallParamatersBecomeRequestUri()
        {
            var call1 = NewEnvironment(
                x => x
                    .Set("owin.RequestPathBase", "/hello")
                    .Set("owin.RequestPath", "/world")
                    .Set("owin.RequestQueryString", "alpha=1&beta=2"),
                x => x
                    .Set("Host", "gamma.com:1234"));

            var message1 = OwinHttpMessageUtils.GetRequestMessage(call1);
            message1.RequestUri.AbsoluteUri.ShouldBe("http://gamma.com:1234/hello/world?alpha=1&beta=2");

            var call2 = NewEnvironment(
                x => x
                    .Set("owin.RequestScheme", "https")
                    .Set("owin.RequestPathBase", "")
                    .Set("owin.RequestPath", "/one/two")
                    .Set("owin.RequestQueryString", ""),
                x => x
                    .Set("Host", "delta.com"));

            var message2 = OwinHttpMessageUtils.GetRequestMessage(call2);
            message2.RequestUri.AbsoluteUri.ShouldBe("https://delta.com/one/two");
        }


        [Fact]
        public void CallHeadersBecomeMessageAndContentHeaders()
        {
            var call = NewEnvironment(
                x => { },
                x => x
                    .Set("Host", "testing")
                    .Set("User-Agent", "Alpha")
                    .Set("Content-Type", "text/plain"));

            var message = OwinHttpMessageUtils.GetRequestMessage(call);
            message.Headers.UserAgent.Single().Product.Name.ShouldBe("Alpha");
            message.Content.Headers.ContentType.MediaType.ShouldBe("text/plain");
        }

    }
}
