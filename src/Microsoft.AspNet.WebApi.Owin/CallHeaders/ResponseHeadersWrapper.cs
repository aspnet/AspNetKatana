using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Microsoft.AspNet.WebApi.Owin.CallHeaders
{
    public class ResponseHeadersWrapper : MessageHeadersWrapper
    {
        private readonly HttpResponseMessage _message;

        public ResponseHeadersWrapper(HttpResponseMessage message)
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

        public HttpResponseMessage Message
        {
            get { return _message; }
        }
    }
}
