using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNet.WebApi.Owin.CallContent
{
    public class HttpContentWrapper
    {
        private readonly HttpContent _content;

        public HttpContentWrapper(HttpContent content)
        {
            _content = content;
        }

        public Task Send(Stream output)
        {
            if (_content == null)
            {
                return TaskHelpers.Completed();
            }

            return _content.CopyToAsync(output);
        }
    }
}