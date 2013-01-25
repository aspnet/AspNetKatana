using System;
using System.IO;
using System.Runtime.Remoting;
using System.Threading;

namespace Microsoft.Owin.Compression.Infrastructure
{
    public abstract class DelegatingStream : Stream
    {
        protected abstract Stream TargetStream { get; }

        public override void Flush()
        {
            TargetStream.Flush();
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return TargetStream.BeginRead(buffer, offset, count, callback, state);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            return TargetStream.EndRead(asyncResult);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return TargetStream.BeginWrite(buffer, offset, count, callback, state);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            TargetStream.EndWrite(asyncResult);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return TargetStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            TargetStream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return TargetStream.Read(buffer, offset, count);
        }

        public override int ReadByte()
        {
            return TargetStream.ReadByte();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            TargetStream.Write(buffer, offset, count);
        }

        public override void WriteByte(byte value)
        {
            TargetStream.WriteByte(value);
        }

        public override bool CanRead
        {
            get { return TargetStream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return TargetStream.CanSeek; }
        }

        public override bool CanTimeout
        {
            get { return TargetStream.CanTimeout; }
        }

        public override bool CanWrite
        {
            get { return TargetStream.CanWrite; }
        }

        public override long Length
        {
            get { return TargetStream.Length; }
        }

        public override long Position
        {
            get { return TargetStream.Position; }
            set { TargetStream.Position = value; }
        }

        public override int ReadTimeout
        {
            get { return TargetStream.ReadTimeout; }
            set { TargetStream.ReadTimeout = value; }
        }

        public override int WriteTimeout
        {
            get { return TargetStream.WriteTimeout; }
            set { TargetStream.WriteTimeout = value; }
        }
    }
}