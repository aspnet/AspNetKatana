using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Owin;

namespace Katana.WebApi.CallContent
{
    public class BodyDelegateWrapper : HttpContent
    {
        private readonly Func<Stream, Task> body;

        public BodyDelegateWrapper(Func<Stream, Task> body)
        {
            this.body = body;
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            return body.Invoke(stream);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }
    }
}