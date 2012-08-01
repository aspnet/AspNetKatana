using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Katana.WebApi.CallHeaders;
using Shouldly;
using Xunit;

namespace Katana.WebApi.Tests
{
    public class MessageHeadersWrapperTests
    {
        private static HttpRequestMessage _message;
        private static RequestHeadersWrapper _headers;

        private static void BuildRequestHeadersWrapper()
        {
            _message = new HttpRequestMessage { Content = new StringContent("Hello") };

            _message.Headers.UserAgent.Add(new ProductInfoHeaderValue("One", "Two"));
            _message.Content.Headers.ContentType.MediaType = "text/plain";

            _headers = new RequestHeadersWrapper(_message);
        }

        [Fact]
        public void HeadersCanBeEnumerated()
        {
            BuildRequestHeadersWrapper();

            var enumerator = _headers.GetEnumerator();
            enumerator.MoveNext().ShouldBe(true);
        }

        [Fact]
        public void HeadersCanBeAdded()
        {
            BuildRequestHeadersWrapper();

            _headers.Add("Accept-Encoding", new[] { "hello" });
            _headers.Add(new KeyValuePair<string, string[]>("Content-Length", new[] { "42" }));

            _message.Content.Headers.ContentLength.ShouldBe(42);
            _message.Headers.AcceptEncoding.ShouldContain(x => x.Value == "hello");
        }

        [Fact]
        public void HeadersCanBeCleared()
        {
            BuildRequestHeadersWrapper();

            _message.Headers.Count().ShouldNotBe(0);
            _message.Content.Headers.Count().ShouldNotBe(0);

            _headers.Clear();

            _message.Headers.Count().ShouldBe(0);
            _message.Content.Headers.Count().ShouldBe(0);
        }

        [Fact]
        public void ContainsCanBeCalled()
        {
            BuildRequestHeadersWrapper();

            _headers.Add("Accept-Encoding", new[] { "hello" });
            _headers.Add(new KeyValuePair<string, string[]>("Content-Length", new[] { "42" }));

            _headers.Contains(new KeyValuePair<string, string[]>("Content-Length", new[] { "42" })).ShouldBe(true);
            _headers.Contains(new KeyValuePair<string, string[]>("Content-Length", new[] { "43" })).ShouldBe(false);
        }

        [Fact]
        public void ContainsKeyCanBeCalled()
        {
            BuildRequestHeadersWrapper();

            _headers.Add("Accept-Encoding", new[] { "hello" });
            _headers.Add(new KeyValuePair<string, string[]>("Content-Length", new[] { "42" }));

            _headers.ContainsKey("Accept-Encoding").ShouldBe(true);
            _headers.ContainsKey("Content-Length").ShouldBe(true);
            _headers.ContainsKey("Content-Type").ShouldBe(true);
            _headers.ContainsKey("Accept-Language").ShouldBe(false);
        }

        [Fact]
        public void RemoveKeyCanBeCalled()
        {
            BuildRequestHeadersWrapper();

            _headers.Add("Accept-Encoding", new[] { "hello" });
            _headers.Add(new KeyValuePair<string, string[]>("Content-Length", new[] { "42" }));

            _headers.Remove("Accept-Encoding").ShouldBe(true);
            _headers.Remove("Content-Length").ShouldBe(true);
            _headers.Remove("Content-Type").ShouldBe(true);
            _headers.Remove("Accept-Language").ShouldBe(false);

            _headers.ContainsKey("Accept-Encoding").ShouldBe(false);
            _headers.ContainsKey("Content-Length").ShouldBe(false);
            _headers.ContainsKey("Content-Type").ShouldBe(false);
            _headers.ContainsKey("Accept-Language").ShouldBe(false);
        }
    }
}
