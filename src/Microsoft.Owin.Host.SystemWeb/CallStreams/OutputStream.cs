// <copyright file="OutputStream.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
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
using System.IO;
#if !NET40
using System.Threading;
using System.Threading.Tasks;
#endif
using System.Web;

namespace Microsoft.Owin.Host.SystemWeb.CallStreams
{
    internal class OutputStream : DelegatingStream
    {
        private readonly HttpResponseBase _response;
        private volatile Action _start;

        internal OutputStream(
            HttpResponseBase response,
            Stream stream,
            Action start)
            : base(stream)
        {
            _response = response;
            _start = start;
        }

        private void Start(bool force)
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

#if !NET40
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            Start(force: false);
            return base.WriteAsync(buffer, offset, count, cancellationToken);
        }
#endif

        public override void Flush()
        {
            Start(force: true);
            base.Flush();
            _response.Flush();
        }

#if !NET40
        public async override Task FlushAsync(CancellationToken cancellationToken)
        {
            Start(force: true);
            await base.FlushAsync(cancellationToken);
            await Task.Factory.FromAsync(_response.BeginFlush, _response.EndFlush, TaskCreationOptions.None);
        }
#endif
    }
}
