using System;
using System.IO;
using System.Net.Http;
using System.Threading;

namespace Katana.WebApi.CallContent
{
    public class HttpContentWrapper
    {
        private readonly HttpContent _content;

        public HttpContentWrapper(HttpContent content)
        {
            _content = content;
        }

        public void Send(
            Func<ArraySegment<byte>, Action, bool> write,
            Action<Exception> end,
            CancellationToken cancellationtoken)
        {
            if (_content == null)
            {
                end(null);
                return;
            }

            var task = _content.CopyToAsync(new WriteStream(write));
            if (task.IsFaulted)
            {
                end(task.Exception);
            }
            else if (task.IsCompleted)
            {
                end(null);
            }
            else
            {
                task.ContinueWith(
                    t =>
                    {
                        if (t.IsFaulted)
                        {
                            end(t.Exception);
                        }
                        else if (t.IsCompleted)
                        {
                            end(null);
                        }
                    });
            }
        }

        public class WriteStream : Stream
        {
            private readonly Func<ArraySegment<byte>, Action, bool> _write;

            public WriteStream(Func<ArraySegment<byte>, Action, bool> write)
            {
                _write = write;
            }

            public override void Flush()
            {
                _write(default(ArraySegment<byte>), null);
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotImplementedException();
            }

            public override void SetLength(long value)
            {
                throw new NotImplementedException();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                throw new NotImplementedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                _write(new ArraySegment<byte>(buffer, offset, count), null);
            }

            public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            {
                return base.BeginWrite(buffer, offset, count, callback, state);
            }

            public override void EndWrite(IAsyncResult asyncResult)
            {
                base.EndWrite(asyncResult);
            }

            public override bool CanRead
            {
                get { return false; }
            }

            public override bool CanSeek
            {
                get { return false; }
            }

            public override bool CanWrite
            {
                get { return true; }
            }

            public override long Length
            {
                get { throw new NotImplementedException(); }
            }

            public override long Position
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }
        }
    }
}