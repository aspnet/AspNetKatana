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
        private readonly BodyDelegate _body;
        private readonly CancellationToken _callDisposed;

        public BodyDelegateWrapper(BodyDelegate body, CancellationToken callDisposed)
        {
            _body = body;
            _callDisposed = callDisposed;
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            return _body.Invoke(stream, _callDisposed);
        }

        protected override bool TryComputeLength(out long length)
        {
            throw new NotImplementedException();
        }
    }
}