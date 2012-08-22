using System;
using System.IO;
using System.Threading;
using System.Web;

namespace Microsoft.AspNet.Owin.CallStreams
{
    public class OutputStream : DelegatingStream
    {
        readonly HttpResponseBase _response;
        readonly Action _start;
        bool _startCalled;
        object _startLock = new object();

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
            if (_response.BufferOutput && !force)
            {
                // don't actually start until flush when response is buffering
                return;
            }

            var ignored = 0;
            LazyInitializer.EnsureInitialized(
                ref ignored,
                ref _startCalled,
                ref _startLock,
                CallStart);
        }

        int CallStart()
        {
            _start();
            return 0;
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
