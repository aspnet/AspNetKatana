// <copyright file="OutputStream.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2011-2013 Microsoft Open Technologies, Inc. All rights reserved.
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
// </copyright>

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

#if !NET40
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
#endif

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

#if !NET40
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
#endif
    }
}
