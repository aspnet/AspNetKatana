// <copyright file="ExceptionFilterStream.cs" company="Katana contributors">
//   Copyright 2011-2013 Katana contributors
// </copyright>
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Diagnostics.Contracts;
using System.IO;
#if !NET40
using System.Threading;
using System.Threading.Tasks;
#endif

namespace Microsoft.Owin.Host.HttpListener
{
    /// <summary>
    /// This class is used to wrap other streams and convert some exception types.
    /// </summary>
    internal abstract class ExceptionFilterStream : Stream
    {
        private readonly Stream _innerStream;

        protected ExceptionFilterStream(Stream innerStream)
        {
            Contract.Requires(innerStream != null);
            _innerStream = innerStream;
        }

        internal Action OnFirstWrite { get; set; }

        #region Properties

        public override bool CanRead
        {
            get { return _innerStream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return _innerStream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return _innerStream.CanWrite; }
        }

        public override long Length
        {
            get { return _innerStream.Length; }
        }

        public override long Position
        {
            get { return _innerStream.Position; }
            set { _innerStream.Position = value; }
        }

        public override bool CanTimeout
        {
            get { return _innerStream.CanTimeout; }
        }

        public override int ReadTimeout
        {
            get { return _innerStream.ReadTimeout; }
            set { _innerStream.ReadTimeout = value; }
        }

        public override int WriteTimeout
        {
            get { return _innerStream.WriteTimeout; }
            set { _innerStream.WriteTimeout = value; }
        }

        #endregion Properties

        protected abstract bool TryWrapException(Exception ex, out Exception wrapped);

        private void FirstWrite()
        {
            Action action = OnFirstWrite;
            if (action != null)
            {
                OnFirstWrite = null;
                action();
            }
        }

        public override void SetLength(long value)
        {
            _innerStream.SetLength(value);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _innerStream.Seek(offset, origin);
        }

#if !NET40
        public override async Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            try
            {
                await _innerStream.CopyToAsync(destination, bufferSize, cancellationToken);
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
#endif

        public override int ReadByte()
        {
            try
            {
                return _innerStream.ReadByte();
            }
            catch (Exception ex)
            {
                Exception wrapped;
                if (TryWrapException(ex, out wrapped))
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
                return _innerStream.Read(buffer, offset, count);
            }
            catch (Exception ex)
            {
                Exception wrapped;
                if (TryWrapException(ex, out wrapped))
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
                return _innerStream.BeginRead(buffer, offset, count, callback, state);
            }
            catch (Exception ex)
            {
                Exception wrapped;
                if (TryWrapException(ex, out wrapped))
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
                return _innerStream.EndRead(asyncResult);
            }
            catch (Exception ex)
            {
                Exception wrapped;
                if (TryWrapException(ex, out wrapped))
                {
                    throw wrapped;
                }

                throw;
            }
        }
        
#if !NET40
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            try
            {
                return await _innerStream.ReadAsync(buffer, offset, count, cancellationToken);
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
#endif

        public override void Write(byte[] buffer, int offset, int count)
        {
            try
            {
                FirstWrite();
                _innerStream.Write(buffer, offset, count);
            }
            catch (Exception ex)
            {
                Exception wrapped;
                if (TryWrapException(ex, out wrapped))
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
                FirstWrite();
                return _innerStream.BeginWrite(buffer, offset, count, callback, state);
            }
            catch (Exception ex)
            {
                Exception wrapped;
                if (TryWrapException(ex, out wrapped))
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
                _innerStream.EndWrite(asyncResult);
            }
            catch (Exception ex)
            {
                Exception wrapped;
                if (TryWrapException(ex, out wrapped))
                {
                    throw wrapped;
                }

                throw;
            }
        }

#if !NET40
        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            try
            {
                this.FirstWrite();
                await _innerStream.WriteAsync(buffer, offset, count, cancellationToken);
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
#endif

        public override void WriteByte(byte value)
        {
            try
            {
                FirstWrite();
                _innerStream.WriteByte(value);
            }
            catch (Exception ex)
            {
                Exception wrapped;
                if (TryWrapException(ex, out wrapped))
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
                FirstWrite();
                _innerStream.Flush();
            }
            catch (Exception ex)
            {
                Exception wrapped;
                if (TryWrapException(ex, out wrapped))
                {
                    throw wrapped;
                }

                throw;
            }
        }

#if !NET40
        public override async Task FlushAsync(CancellationToken cancellationToken)
        {
            try
            {
                this.FirstWrite();
                await _innerStream.FlushAsync(cancellationToken);
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
#endif

        public override void Close()
        {
            try
            {
                FirstWrite();
                _innerStream.Close();
            }
            catch (Exception ex)
            {
                Exception wrapped;
                if (TryWrapException(ex, out wrapped))
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
                _innerStream.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
