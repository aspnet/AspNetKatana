// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Microsoft.Owin.Host.SystemWeb.CallStreams
{
    internal class OutputStream : DelegatingStream
    {
        private readonly HttpResponseBase _response;
        private volatile Action _start;
        private Action _faulted;

        internal OutputStream(
            HttpResponseBase response,
            Stream stream,
            Action start,
            Action faulted)
            : base(stream)
        {
            _response = response;
            _start = start;
            _faulted = faulted;
        }

        private void Start(bool force)
        {
            Action start = _start;
            if (start == null || (!force && _response.BufferOutput))
            {
                return;
            }

            start();
            _start = null;
        }

        private void Faulted()
        {
            Interlocked.Exchange(ref _faulted, () => { }).Invoke();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            try
            {
                Start(force: false);
                base.Write(buffer, offset, count);
            }
            catch (HttpException)
            {
                Faulted();
                throw;
            }
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, System.AsyncCallback callback, object state)
        {
            try
            {
                Start(force: false);
                return base.BeginWrite(buffer, offset, count, callback, state);
            }
            catch (HttpException)
            {
                Faulted();
                throw;
            }
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            try
            {
                base.EndWrite(asyncResult);
            }
            catch (HttpException)
            {
                Faulted();
                throw;
            }
        }

        public override void WriteByte(byte value)
        {
            try
            {
                Start(force: false);
                base.WriteByte(value);
            }
            catch (HttpException)
            {
                Faulted();
                throw;
            }
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            try
            {
                Start(force: false);
                return base.WriteAsync(buffer, offset, count, cancellationToken);
            }
            catch (HttpException)
            {
                Faulted();
                throw;
            }
        }

        public override void Flush()
        {
            try
            {
                Start(force: true);
                _response.Flush();
            }
            catch (HttpException)
            {
                Faulted();
                throw;
            }
        }

        public override async Task FlushAsync(CancellationToken cancellationToken)
        {
            try
            {
                Start(force: true);
                await Task.Factory.FromAsync(_response.BeginFlush, _response.EndFlush, TaskCreationOptions.None);
            }
            catch (HttpException)
            {
                Faulted();
                throw;
            }
        }
    }
}
