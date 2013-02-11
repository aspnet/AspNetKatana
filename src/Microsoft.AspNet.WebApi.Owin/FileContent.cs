// <copyright file="FileContent.cs" company="Microsoft Open Technologies, Inc.">
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNet.WebApi.Owin
{
    using SendFileFunc = Func<string, long, long?, CancellationToken, Task>;

    /// <summary>
    /// A prototype HttpContent that demonstrates how to efficiently send static files via WebApi+Owin.
    /// </summary>
    public class FileContent : HttpContent
    {
        private const int DefaultBufferSize = 1024 * 64;

        private readonly string _fileName;
        private readonly long _offset;
        private readonly long? _count;
        private readonly FileInfo _fileInfo;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        public FileContent(string fileName)
            : this(fileName, 0, null)
        {
        }

        // TODO: Multiple ranges via multipart?

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public FileContent(string fileName, long offset, long? count)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentNullException("fileName");
            }
            _fileInfo = new FileInfo(fileName);
            if (!_fileInfo.Exists)
            {
                throw new FileNotFoundException(string.Empty, fileName);
            }
            if (offset < 0 || offset > _fileInfo.Length)
            {
                throw new ArgumentOutOfRangeException("offset", offset, string.Empty);
            }
            if (count.HasValue && count.Value - offset > _fileInfo.Length)
            {
                throw new ArgumentOutOfRangeException("count", count.Value, string.Empty);
            }

            _fileName = fileName;
            _offset = offset;
            _count = count;

            long length = _count ?? _fileInfo.Length - _offset;
            long end = _offset + length - 1;
            Headers.ContentRange = new ContentRangeHeaderValue(_offset, end, _fileInfo.Length);
        }

        /// <summary>
        /// 
        /// </summary>
        public string FileName
        {
            get { return _fileName; }
        }

        /// <summary>
        /// 
        /// </summary>
        public long Offset
        {
            get { return _offset; }
        }

        /// <summary>
        /// 
        /// </summary>
        public long? Count
        {
            get { return _count; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        protected override bool TryComputeLength(out long length)
        {
            length = _count ?? _fileInfo.Length - _offset;
            return true;
        }

        // Normal stream copy
        // TODO: Multiple ranges via multipart?

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            using (var fileStream =
                new FileStream(_fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, DefaultBufferSize,
                    FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                fileStream.Seek(_offset, SeekOrigin.Begin);
                // await fileStream.CopyToAsync(stream, bufferSize, _count);
            }
            throw new NotImplementedException();
        }

        // TODO: Multiple ranges via multipart?

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="sendFileFunc"></param>
        /// <param name="cancel"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public Task SendFileAsync(Stream stream, SendFileFunc sendFileFunc, CancellationToken cancel)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (sendFileFunc == null)
            {
                throw new ArgumentNullException("sendFileFunc");
            }

            return sendFileFunc(_fileName, _offset, _count, cancel);
        }
    }
}
