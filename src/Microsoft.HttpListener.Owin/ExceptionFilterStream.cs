//-----------------------------------------------------------------------
// <copyright>
//   Copyright (c) Katana Contributors. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Diagnostics.Contracts;
using System.IO;

namespace Microsoft.HttpListener.Owin
{
    /// <summary>
    /// This class is used to wrap other streams and convert some exception types.
    /// </summary>
    internal abstract class ExceptionFilterStream : Stream
    {
        private Stream innerStream;

        protected ExceptionFilterStream(Stream innerStream)
        {
            Contract.Requires(innerStream != null);
            this.innerStream = innerStream;
        }

        internal Action OnFirstWrite { get; set; }

        #region Properties

        public override bool CanRead
        {
            get { return this.innerStream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return this.innerStream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return this.innerStream.CanWrite; }
        }

        public override long Length
        {
            get { return this.innerStream.Length; }
        }

        public override long Position
        {
            get { return this.innerStream.Position; }
            set { this.innerStream.Position = value; }
        }

        public override bool CanTimeout
        {
            get { return this.innerStream.CanTimeout; }
        }

        public override int ReadTimeout
        {
            get { return this.innerStream.ReadTimeout; }
            set { this.innerStream.ReadTimeout = value; }
        }

        public override int WriteTimeout
        {
            get { return this.innerStream.WriteTimeout; }
            set { this.innerStream.WriteTimeout = value; }
        }

        #endregion Properties

        protected abstract bool TryWrapException(Exception ex, out Exception wrapped);

        private void FirstWrite()
        {
            Action action = this.OnFirstWrite;
            if (action != null)
            {
                this.OnFirstWrite = null;
                action();
            }
        }

        public override void SetLength(long value)
        {
            this.innerStream.SetLength(value);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return this.innerStream.Seek(offset, origin);
        }

        /* .NET 4.5
        public override async Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            try
            {
                await this.innerStream.CopyToAsync(destination, bufferSize, cancellationToken);
            }
            catch (Exception ex)
            {
                Exception wrapped;
                if (this.TryWrapException(ex, out wrapped))
                {
                    throw wrapped;
                }

                throw;
            }
        }
        */
        public override int ReadByte()
        {
            try
            {
                return this.innerStream.ReadByte();
            }
            catch (Exception ex)
            {
                Exception wrapped;
                if (this.TryWrapException(ex, out wrapped))
                {
                    throw wrapped;
                }

                throw;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            try
            {
                return this.innerStream.Read(buffer, offset, count);
            }
            catch (Exception ex)
            {
                Exception wrapped;
                if (this.TryWrapException(ex, out wrapped))
                {
                    throw wrapped;
                }

                throw;
            }
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            try
            {
                return this.innerStream.BeginRead(buffer, offset, count, callback, state);
            }
            catch (Exception ex)
            {
                Exception wrapped;
                if (this.TryWrapException(ex, out wrapped))
                {
                    throw wrapped;
                }

                throw;
            }
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            try
            {
                return this.innerStream.EndRead(asyncResult);
            }
            catch (Exception ex)
            {
                Exception wrapped;
                if (this.TryWrapException(ex, out wrapped))
                {
                    throw wrapped;
                }

                throw;
            }
        }

        /* .NET 4.5
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            try
            {
                return await this.innerStream.ReadAsync(buffer, offset, count, cancellationToken);
            }
            catch (Exception ex)
            {
                Exception wrapped;
                if (this.TryWrapException(ex, out wrapped))
                {
                    throw wrapped;
                }

                throw;
            }
        }
        */
        public override void Write(byte[] buffer, int offset, int count)
        {
            try
            {
                this.FirstWrite();
                this.innerStream.Write(buffer, offset, count);
            }
            catch (Exception ex)
            {
                Exception wrapped;
                if (this.TryWrapException(ex, out wrapped))
                {
                    throw wrapped;
                }

                throw;
            }
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            try
            {
                this.FirstWrite();
                return this.innerStream.BeginWrite(buffer, offset, count, callback, state);
            }
            catch (Exception ex)
            {
                Exception wrapped;
                if (this.TryWrapException(ex, out wrapped))
                {
                    throw wrapped;
                }

                throw;
            }
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            try
            {
                this.innerStream.EndWrite(asyncResult);
            }
            catch (Exception ex)
            {
                Exception wrapped;
                if (this.TryWrapException(ex, out wrapped))
                {
                    throw wrapped;
                }

                throw;
            }
        }

        /* // .NET 4.5
        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            try
            {
                this.FirstWrite();
                await this.innerStream.WriteAsync(buffer, offset, count, cancellationToken);
            }
            catch (Exception ex)
            {
                Exception wrapped;
                if (this.TryWrapException(ex, out wrapped))
                {
                    throw wrapped;
                }

                throw;
            }
        }
        */
        public override void WriteByte(byte value)
        {
            try
            {
                this.FirstWrite();
                this.innerStream.WriteByte(value);
            }
            catch (Exception ex)
            {
                Exception wrapped;
                if (this.TryWrapException(ex, out wrapped))
                {
                    throw wrapped;
                }

                throw;
            }
        }

        public override void Flush()
        {
            try
            {
                this.FirstWrite();
                this.innerStream.Flush();
            }
            catch (Exception ex)
            {
                Exception wrapped;
                if (this.TryWrapException(ex, out wrapped))
                {
                    throw wrapped;
                }

                throw;
            }
        }

        /* .NET 4.5
        public override async Task FlushAsync(CancellationToken cancellationToken)
        {
            try
            {
                this.FirstWrite();
                await this.innerStream.FlushAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Exception wrapped;
                if (this.TryWrapException(ex, out wrapped))
                {
                    throw wrapped;
                }

                throw;
            }
        }
        */
        public override void Close()
        {
            try
            {
                this.FirstWrite();
                this.innerStream.Close();
            }
            catch (Exception ex)
            {
                Exception wrapped;
                if (this.TryWrapException(ex, out wrapped))
                {
                    throw wrapped;
                }

                throw;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.innerStream.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
