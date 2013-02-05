// <copyright file="StaticCompressionContext.cs" company="Katana contributors">
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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin.Compression.Encoding;
using Microsoft.Owin.Compression.Infrastructure;
using Microsoft.Owin.Compression.Storage;
using Owin.Types;
using Owin.Types.Helpers;

namespace Microsoft.Owin.Compression
{
    internal class StaticCompressionContext
    {
        private readonly IDictionary<string, object> _environment;
        private readonly StaticCompressionOptions _options;
        private readonly IEncoding _encoding;
        private readonly string _encodingSuffix;
        private readonly string _encodingSuffixQuote;
        private readonly ICompressedStorage _storage;
        private OwinRequest _request;
        private OwinResponse _response;
        private Stream _originalResponseBody;
        private Func<string, long, long?, CancellationToken, Task> _originalSendFileAsyncDelegate;
        private InterceptMode _intercept;
        private bool _interceptResponse;
        private bool _interceptInitialized;
        private object _interceptLock = new object();
        private Stream _compressingStream;
        private ICompressedItemBuilder _compressedItemBuilder;
        private static readonly Func<InterceptMode> InterceptDetaching = () => InterceptMode.DoingNothing;
        private string _compressedETag;
        private ICompressedItemHandle _compressedItem;
        private string[] _originalIfNoneMatch;
        private string[] _originalIfMatch;

        private static readonly StringSegment CommaSegment = new StringSegment(", ", 0, 2);
        private static readonly StringSegment QuoteSegment = new StringSegment("\"", 0, 1);

        public StaticCompressionContext(IDictionary<string, object> environment, StaticCompressionOptions options, IEncoding encoding, ICompressedStorage storage)
        {
            _environment = environment;
            _options = options;
            _encoding = encoding;
            _encodingSuffix = "^" + _encoding.Name;
            _encodingSuffixQuote = "^" + _encoding.Name + "\"";
            _storage = storage;
            _request = new OwinRequest(environment);
            _response = new OwinResponse(environment);
        }

        internal enum InterceptMode
        {
            Uninitialized,
            DoingNothing,
            CompressingToStorage,
            SentFromStorage,
        }

        public void Attach()
        {
            // TODO: look to see if this Vary is already added?
            _response.AddHeaderJoined("Vary", "Accept-Encoding");

            _originalIfNoneMatch = CleanRequestHeader("If-None-Match");
            _originalIfMatch = CleanRequestHeader("If-Match");

            _originalResponseBody = _response.Body;
            _response.Body = new SwitchingStream(this, _originalResponseBody);

            _originalSendFileAsyncDelegate = _response.SendFileAsyncDelegate;
            _response.SendFileAsyncDelegate = SendFileASync;
        }

        private string[] CleanRequestHeader(string name)
        {
            string[] original = _request.GetHeaderUnmodified(name);
            if (original != null)
            {
                var tacking = new Tacking();
                bool modified = false;
                foreach (var segment in new HeaderSegments(original))
                {
                    if (segment.Data.HasValue)
                    {
                        if (segment.Data.EndsWith(_encodingSuffixQuote, StringComparison.Ordinal))
                        {
                            modified = true;
                            if (!tacking.IsEmpty)
                            {
                                tacking.Add(CommaSegment);
                            }
                            tacking.Add(segment.Data.Subsegment(0, segment.Data.Count - _encodingSuffixQuote.Length));
                            tacking.Add(QuoteSegment);
                        }
                        else if (segment.Data.EndsWith(_encodingSuffix, StringComparison.Ordinal))
                        {
                            modified = true;
                            if (!tacking.IsEmpty)
                            {
                                tacking.Add(CommaSegment);
                            }
                            tacking.Add(segment.Data.Subsegment(0, segment.Data.Count - _encodingSuffix.Length));
                        }
                        else
                        {
                            if (!tacking.IsEmpty)
                            {
                                tacking.Add(CommaSegment);
                            }
                            tacking.Add(segment.Data);
                        }
                    }
                }
                if (modified)
                {
                    _request.SetHeader(name, tacking.BuildString());
                    return original;
                }
            }
            return null;
        }

        private void Detach()
        {
            Intercept(detaching: true);
            _response.Body = _originalResponseBody;
            _response.SendFileAsyncDelegate = _originalSendFileAsyncDelegate;
            if (_originalIfNoneMatch != null)
            {
                _request.SetHeaderUnmodified("If-None-Match", _originalIfNoneMatch);
            }
            if (_originalIfMatch != null)
            {
                _request.SetHeaderUnmodified("If-Match", _originalIfMatch);
            }
        }

        public InterceptMode Intercept(bool detaching = false)
        {
            return LazyInitializer.EnsureInitialized(
                ref _intercept,
                ref _interceptInitialized,
                ref _interceptLock,
                detaching ? InterceptDetaching : InterceptOnce);
        }

        public InterceptMode InterceptOnce()
        {
            StringSegment etag = SingleSegment(_response, "ETag");

            if (!etag.HasValue)
            {
                return InterceptMode.DoingNothing;
            }

            if (etag.StartsWith("\"", StringComparison.Ordinal) &&
                etag.EndsWith("\"", StringComparison.Ordinal))
            {
                _compressedETag = etag.Substring(0, etag.Count - 1) + "^" + _encoding.Name + "\"";
            }
            else
            {
                _compressedETag = "\"" + etag.Value + "^" + _encoding.Name + "\"";
            }

            int statusCode = _response.StatusCode;
            if (statusCode == 304)
            {
                return InterceptMode.SentFromStorage;
            }

            var key = new CompressedKey
            {
                ETag = _compressedETag,
                RequestPath = _request.Path,
                RequestQueryString = _request.QueryString,
                RequestMethod = _request.Method,
            };

            _compressedItem = _storage.Open(key);
            if (_compressedItem != null)
            {
                return InterceptMode.SentFromStorage;
            }

            _compressedItemBuilder = _storage.Create(key);
            _compressingStream = _encoding.CompressTo(_compressedItemBuilder.Stream);
            return InterceptMode.CompressingToStorage;
        }

        private StringSegment SingleSegment(OwinResponse response, string header)
        {
            HeaderSegments.Enumerator cursor = new HeaderSegments(response.GetHeaderUnmodified(header)).GetEnumerator();
            if (cursor.MoveNext())
            {
                HeaderSegment segment = cursor.Current;
                if (cursor.MoveNext() == false)
                {
                    return segment.Data;
                }
            }
            return new StringSegment();
        }

        public Stream GetTargetStream()
        {
            switch (Intercept())
            {
                case InterceptMode.DoingNothing:
                    return _originalResponseBody;
                case InterceptMode.CompressingToStorage:
                    return _compressingStream;
                case InterceptMode.SentFromStorage:
                    return Stream.Null;
            }
            // bad value
            throw new NotImplementedException();
        }

        public Task Complete()
        {
            InterceptMode interceptMode = Intercept();
            Detach();

            switch (interceptMode)
            {
                case InterceptMode.DoingNothing:
                    return TaskHelpers.Completed();
                case InterceptMode.CompressingToStorage:
                    _compressingStream.Close();
                    _compressedItem = _storage.Commit(_compressedItemBuilder);
                    _response.SetHeader("Content-Length", _compressedItem.CompressedLength.ToString(CultureInfo.InvariantCulture));
                    _response.SetHeader("ETag", _compressedETag);
                    _response.SetHeader("Content-Encoding", _encoding.Name);
                    if (_compressedItem.PhysicalPath != null && _originalSendFileAsyncDelegate != null)
                    {
                        return _originalSendFileAsyncDelegate.Invoke(_compressedItem.PhysicalPath, 0, _compressedItem.CompressedLength, _request.CallCancelled);
                    }
                    else
                    {
                        // TODO: stream copy operation
                    }
                    return TaskHelpers.Completed();
                case InterceptMode.SentFromStorage:
                    _response.SetHeader("ETag", _compressedETag);
                    _response.SetHeader("Content-Encoding", _encoding.Name);
                    if (_compressedItem != null)
                    {
                        _response.SetHeader("Content-Length", _compressedItem.CompressedLength.ToString(CultureInfo.InvariantCulture));
                        if (_compressedItem.PhysicalPath != null && _originalSendFileAsyncDelegate != null)
                        {
                            return _originalSendFileAsyncDelegate.Invoke(_compressedItem.PhysicalPath, 0, _compressedItem.CompressedLength, _request.CallCancelled);
                        }
                        else
                        {
                            // TODO: stream copy operation
                        }
                    }
                    return TaskHelpers.Completed();
            }

            throw new NotImplementedException();
        }

        public CatchInfoBase<Task>.CatchResult Complete(CatchInfo catchInfo)
        {
            Detach();
            return catchInfo.Throw();
        }

        private Task SendFileASync(string fileName, long offset, long? count, CancellationToken cancel)
        {
            switch (Intercept())
            {
                case InterceptMode.DoingNothing:
                    {
                        if (_originalSendFileAsyncDelegate != null)
                        {
                            return _originalSendFileAsyncDelegate.Invoke(fileName, offset, count, cancel);
                        }

                        // TODO: sync errors go faulted task
                        var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        fileStream.Seek(offset, SeekOrigin.Begin);
                        var copyOperation = new StreamCopyOperation(fileStream, _originalResponseBody, count, cancel);
                        return copyOperation.Start().Finally(fileStream.Close);
                    }
                case InterceptMode.CompressingToStorage:
                    {
                        // TODO: sync errors go faulted task
                        var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        fileStream.Seek(offset, SeekOrigin.Begin);
                        var copyOperation = new StreamCopyOperation(fileStream, _compressingStream, count, cancel);
                        return copyOperation.Start().Finally(fileStream.Close);
                    }
                case InterceptMode.SentFromStorage:
                    return TaskHelpers.Completed();
            }
            throw new NotImplementedException();
        }

        private struct Tacking
        {
            private List<StringSegment> _segments;
            private int _length;

            public bool IsEmpty
            {
                get { return _length == 0; }
            }

            public void Add(StringSegment segment)
            {
                if (segment.Count == 0)
                {
                    return;
                }
                if (_segments == null)
                {
                    _segments = new List<StringSegment>();
                }
                _segments.Add(segment);
                _length += segment.Count;
            }

            public string BuildString()
            {
                var sb = new StringBuilder(_length, _length);
                foreach (var segment in _segments)
                {
                    sb.Append(segment.Buffer, segment.Offset, segment.Count);
                }
                return sb.ToString();
            }
        }
    }
}
