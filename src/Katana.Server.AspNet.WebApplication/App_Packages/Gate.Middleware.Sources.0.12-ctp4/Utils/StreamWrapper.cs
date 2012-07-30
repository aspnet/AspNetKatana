using System;
using System.IO;
using System.Runtime.Remoting;
using System.Threading;

namespace Gate.Middleware.Utils
{
    internal class StreamWrapper : Stream
    {
        readonly Stream _inner;
        readonly Func<ArraySegment<byte>, ArraySegment<byte>[]> _writeFilter;

        public StreamWrapper(Stream inner, Func<ArraySegment<byte>, ArraySegment<byte>[]> writeFilter)
        {
            _inner = inner;
            _writeFilter = writeFilter;
        }

        public override void Close()
        {
            _inner.Close();
        }


        public override void Flush()
        {
            _inner.Flush();
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return _inner.BeginRead(buffer, offset, count, callback, state);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            return _inner.EndRead(asyncResult);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            var segments = _writeFilter(new ArraySegment<byte>(buffer, offset, count));
            if (segments.Length == 0)
            {
                segments = new[] { new ArraySegment<byte>(new byte[0]) };
            }

            for (var index = 0; index != segments.Length - 1; ++index)
            {
                _inner.Write(segments[index].Array, segments[index].Offset, segments[index].Count);
            }

            var last = segments[segments.Length - 1];
            return _inner.BeginWrite(last.Array, last.Offset, last.Count, callback, state);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            _inner.EndWrite(asyncResult);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _inner.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _inner.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _inner.Read(buffer, offset, count);
        }

        //public override int ReadByte()
        //{
        //    return _inner.ReadByte();
        //}

        public override void Write(byte[] buffer, int offset, int count)
        {
            foreach (var segment in _writeFilter(new ArraySegment<byte>(buffer, offset, count)))
            {
                _inner.Write(segment.Array, segment.Offset, segment.Count);
            }
        }

        //public override void WriteByte(byte value)
        //{
        //    _inner.WriteByte(value);
        //}


        public override bool CanRead
        {
            get { return _inner.CanRead; }
        }

        public override bool CanSeek
        {
            get { return _inner.CanSeek; }
        }

        public override bool CanTimeout
        {
            get { return _inner.CanTimeout; }
        }

        public override bool CanWrite
        {
            get { return _inner.CanWrite; }
        }

        public override long Length
        {
            get { return _inner.Length; }
        }

        public override long Position
        {
            get { return _inner.Position; }
            set { _inner.Position = value; }
        }

        public override int ReadTimeout
        {
            get { return _inner.ReadTimeout; }
            set { _inner.ReadTimeout = value; }
        }

        public override int WriteTimeout
        {
            get { return _inner.WriteTimeout; }
            set { _inner.WriteTimeout = value; }
        }

        public override string ToString()
        {
            return _inner.ToString();
        }
    }
}
