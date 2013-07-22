// <copyright file="InputStream.cs" company="Microsoft Open Technologies, Inc.">
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

#if !NET40

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Microsoft.Owin.Host.SystemWeb.CallStreams
{
    internal class InputStream : DelegatingStream
    {
        private readonly HttpRequestBase _request;
        private Stream _stream;
        private bool _preferBuffered = true;
        private bool _bufferOnSeek = false;

        internal InputStream(HttpRequestBase request)
        {
            _request = request;
        }

        protected override Stream Stream
        {
            get
            {
                ResolveStream();
                return _stream;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override bool CanSeek
        {
            get
            {
                switch (_request.ReadEntityBodyMode)
                {
                    case ReadEntityBodyMode.None:
                    case ReadEntityBodyMode.Buffered:
                        return _preferBuffered;
                    case ReadEntityBodyMode.Classic:
                        return true;                            
                    case ReadEntityBodyMode.Bufferless:
                        return false;
                    default:
                        throw new NotImplementedException(_request.ReadEntityBodyMode.ToString());
                }
            }
        }

        public override long Position
        {
            get
            {
                if (_stream == null)
                {
                    // Workaround for WebAPI StreamContent. It records the position even if it doesn't consume the stream.
                    return 0;
                }
                return base.Position;
            }
            set
            {
                Seek(value, SeekOrigin.Begin);
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            ResolveStream();
            // Don't buffer to seek if nothing would change (e.g. it was already at the beginning).
            if (_bufferOnSeek // ReadEntityBodyMode.Buffered & _preferBuffered
                && !(origin == SeekOrigin.Begin && offset == _stream.Position))
            {
                long position = _stream.Position;
                byte[] ignored = new byte[1024];
                while (_stream.Read(ignored, 0, ignored.Length) > 0)
                {
                }
                _stream = _request.InputStream;
                _stream.Position = position;
                _bufferOnSeek = false;
            }
            return base.Seek(offset, origin);
        }

        private void ResolveStream()
        {
            if (_stream != null)
            {
                return;
            }
            switch (_request.ReadEntityBodyMode)
            {
                case ReadEntityBodyMode.None:
                    _stream = _preferBuffered
                        ? _request.GetBufferedInputStream()
                        : _request.GetBufferlessInputStream();
                    _bufferOnSeek = _preferBuffered;
                    break;
                case ReadEntityBodyMode.Buffered:
                    _stream = _request.GetBufferedInputStream();
                    try
                    {
                        // Fully consumed? Switch to Seekable InputStream.
                        _stream = _request.InputStream;
                    }
                    catch (InvalidOperationException)
                    {
                        // The stream is starting in a partially read state.
                        _bufferOnSeek = _preferBuffered;
                    }
                    break;
                case ReadEntityBodyMode.Classic:
                    _stream = _request.InputStream;
                    break;
                case ReadEntityBodyMode.Bufferless:
                    _stream = _request.GetBufferlessInputStream();
                    break;
                default:
                    throw new NotImplementedException(_request.ReadEntityBodyMode.ToString());
            }
        }

        internal void DisableBuffering()
        {
            switch (_request.ReadEntityBodyMode)
            {
                case ReadEntityBodyMode.Classic:
                case ReadEntityBodyMode.Buffered:
                    // Too late, already buffering. (But let them disable Seek buffering if they want).
                case ReadEntityBodyMode.None:
                    _preferBuffered = false;
                    _bufferOnSeek = false;
                    break;
                case ReadEntityBodyMode.Bufferless:
                    break; // Already disabled
                default:
                    throw new NotImplementedException(_request.ReadEntityBodyMode.ToString());
            }
        }
    }
}
#endif