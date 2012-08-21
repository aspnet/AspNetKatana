using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Owin;

namespace Microsoft.AspNet.WebApi.Owin.CallContent
{
    public class BodyStreamContent : StreamContent
    {
        private readonly Stream _body;

        public BodyStreamContent(Stream body) : base(body)
        {
            _body = body;
        }

        public Stream Body
        {
            get { return _body; }
        }

        protected override Task<Stream> CreateContentReadStreamAsync()
        {
            return TaskHelpers.FromResult(Body);
        }
 
        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }
    }
}