using System;
using System.IO;
using System.Web;

namespace Microsoft.AspNet.Owin.CallStreams
{
    public class OutputStream : DelegatingStream
    {
        readonly HttpResponseBase _response;
        volatile Action _start;

        public OutputStream(
            HttpResponseBase response,
            Stream stream,
            Action start)
            : base(stream)
        {
            _response = response;
            _start = start;
        }

        void Start(bool force)
        {
            var start = _start;
            if (start == null || (!force && _response.BufferOutput))
            {
                return;
            }

            start();
            _start = null;
        }


        public override void Write(byte[] buffer, int offset, int count)
        {
            Start(force: false);
            base.Write(buffer, offset, count);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, System.AsyncCallback callback, object state)
        {
            Start(force: false);
            return base.BeginWrite(buffer, offset, count, callback, state);
        }

        public override void WriteByte(byte value)
        {
            Start(force: false);
            base.WriteByte(value);
        }

        public override void Flush()
        {
            Start(force: true);
            base.Flush();
            _response.Flush();
        }
    }
}
