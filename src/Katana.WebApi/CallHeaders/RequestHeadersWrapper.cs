using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Katana.WebApi.CallHeaders
{
    public class RequestHeadersWrapper : MessageHeadersWrapper
    {
        private readonly HttpRequestMessage _message;

        public RequestHeadersWrapper(HttpRequestMessage message)
        {
            _message = message;
        }

        protected override HttpHeaders MessageHeaders
        {
            get { return _message.Headers; }
        }

        protected override HttpHeaders ContentHeaders
        {
            get { return _message.Content != null ? _message.Content.Headers : null; }
        }

        public HttpRequestMessage Message
        {
            get { return _message; }
        }
    }
}