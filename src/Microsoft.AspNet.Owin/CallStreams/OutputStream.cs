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
using System.Web;

namespace Microsoft.Owin.Host.SystemWeb.CallStreams
{
    public class OutputStream : DelegatingStream
    {
        private readonly HttpResponseBase _response;
        private volatile Action _start;

        public OutputStream(
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

        public override void Flush()
        {
            Start(force: true);
            base.Flush();
            _response.Flush();
        }
    }
}
