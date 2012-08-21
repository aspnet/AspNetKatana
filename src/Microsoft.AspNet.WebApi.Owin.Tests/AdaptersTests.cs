using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Owin;
using Shouldly;
using Xunit;

namespace Microsoft.AspNet.WebApi.Owin.Tests
{
    public class AdaptersTests
    {
        [Fact]
        public void OwinCallWillBecomeRequestMessage()
        {
            var call = new CallParameters
            {
                Environment = new Dictionary<string, object>
                {
                    {"owin.RequestMethod", "POST"},
                    {"owin.RequestScheme", "http"},
                },
                Headers = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase),
                Body = new MemoryStream()
            };

            var requestMessage = Utils.GetRequestMessage(call);
            requestMessage.Method.Method.ShouldBe("POST");
        }

        [Fact]
        public void CallParamatersBecomeRequestUri()
        {
            var call1 = new CallParameters
            {
                Environment = new Dictionary<string, object>
                {
                    {"owin.RequestMethod", "POST"},
                    {"owin.RequestScheme", "http"},
                    {"owin.RequestPathBase", "/hello"},
                    {"owin.RequestPath", "/world"},
                    {"owin.RequestQueryString", "alpha=1&beta=2"},
                },
                Headers = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
                {
                    {"Host", new[]{"gamma.com:1234"}}
                },
                Body = new MemoryStream()
            };

            var message1 = Utils.GetRequestMessage(call1);
            message1.RequestUri.AbsoluteUri.ShouldBe("http://gamma.com:1234/hello/world?alpha=1&beta=2");

            var call2 = new CallParameters
            {
                Environment = new Dictionary<string, object>
                {
                    {"owin.RequestMethod", "POST"},
                    {"owin.RequestScheme", "https"},
                    {"owin.RequestPathBase", ""},
                    {"owin.RequestPath", "/one/two"},
                    {"owin.RequestQueryString", ""},
                },
                Headers = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
                {
                    {"Host", new[]{"delta.com"}}
                },
                Body = new MemoryStream()
            };

            var message2 = Utils.GetRequestMessage(call2);
            message2.RequestUri.AbsoluteUri.ShouldBe("https://delta.com/one/two");
        }


        [Fact]
        public void CallHeadersBecomeMessageAndContentHeaders()
        {
            var call = new CallParameters
            {
                Environment = new Dictionary<string, object>
                {
                    {"owin.RequestMethod", "POST"},
                    {"owin.RequestScheme", "http"},
                    {"owin.RequestPathBase", ""},
                    {"owin.RequestPath", "/"},
                    {"owin.RequestQueryString", ""},                
                },
                Headers = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
                {
                    {"Host", new[]{"testing"}},
                    {"User-Agent", new []{"Alpha"}},
                    {"Content-Type", new []{"text/plain"}},
                },
                Body = new MemoryStream()
            };

            var message = Utils.GetRequestMessage(call);
            message.Headers.UserAgent.Single().Product.Name.ShouldBe("Alpha");
            message.Content.Headers.ContentType.MediaType.ShouldBe("text/plain");
        }

        [Fact]
        public Task CallParametersWillRoundTripWithNewHeadersCollection()
        {
            var call1 = new CallParameters
            {
                Environment = new Dictionary<string, object>
                {
                    {"owin.RequestMethod", "POST"},
                    {"owin.RequestScheme", "http"},
                    {"owin.RequestPathBase", ""},
                    {"owin.RequestPath", "/"},
                    {"owin.RequestQueryString", ""},                
                },
                Headers = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
                {
                    {"Host", new[]{"testing"}},
                    {"User-Agent", new []{"Alpha"}},
                    {"Content-Type", new []{"text/plain"}},
                },
                Body = new MemoryStream()
            };
            var message = Utils.GetRequestMessage(call1);
            return Utils.GetCallParameters(message)
                .Then(call2 =>
                {
                    call2.Environment.ShouldBeSameAs(call1.Environment);
                    call2.Headers.ShouldNotBeSameAs(call1.Headers);
                    call2.Body.ShouldBeSameAs(call1.Body);

                    call2.Headers.ShouldContainKey("Host");
                    call2.Headers.ShouldContainKey("User-Agent");
                    call2.Headers.ShouldContainKey("Content-Type");
                });
        }

        [Fact]
        public Task ChangingStreamWillCauseNewContentButPreserveHeaders()
        {
            var call1 = new CallParameters
            {
                Environment = new Dictionary<string, object>
                {
                    {"owin.RequestMethod", "POST"},
                    {"owin.RequestScheme", "http"},
                    {"owin.RequestPathBase", ""},
                    {"owin.RequestPath", "/"},
                    {"owin.RequestQueryString", ""},                
                },
                Headers = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
                {
                    {"Host", new[]{"testing"}},
                    {"User-Agent", new []{"Alpha"}},
                    {"Content-Type", new []{"text/plain"}},
                },
                Body = new MemoryStream()
            };
            var message1 = Utils.GetRequestMessage(call1);
            var content1 = message1.Content;
            message1.Content.Headers.Add("x-custom", "delta");
            return Utils.GetCallParameters(message1)
                .Then(call2 =>
                {
                    call2.Body = new MemoryStream(new byte[] { 65, 66, 67 });
                    var message2 = Utils.GetRequestMessage(call2);
                    
                    message2.ShouldBeSameAs(message1);
                    message2.Content.ShouldNotBeSameAs(content1);

                    message2.Content.ReadAsStringAsync().Then(
                        data =>
                        {
                            data.ShouldBe("ABC");
                            message2.Content.Headers.ShouldContain(x => x.Key == "x-custom");
                        });
                });
        }
    }
}
