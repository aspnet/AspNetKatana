using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
    public class StaticCompressionMiddleware
    {
        private readonly Func<IDictionary<string, object>, Task> _next;
        private readonly StaticCompressionOptions _options;

        private ICompressedStorage _storage;
        private bool _storageInitialized;
        private object _storageLock = new object();

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public StaticCompressionMiddleware(Func<IDictionary<string, object>, Task> next, StaticCompressionOptions options)
        {
            _next = next;
            _options = options;
        }

        public Task Invoke(IDictionary<string, object> environment)
        {
            var compression = SelectCompression(environment);
            if (compression == null)
            {
                return _next(environment);
            }

            var storage = GetStorage(environment);
            if (storage == null)
            {
                return _next(environment);
            }

            var context = new StaticCompressionContext(environment, _options, compression, storage);
            context.Attach();
            return _next(environment)
                .Then((Func<Task>)context.Complete)
                .Catch(context.Complete);
        }

        private ICompressedStorage GetStorage(IDictionary<string, object> environment)
        {
            return LazyInitializer.EnsureInitialized(
                ref _storage,
                ref _storageInitialized,
                ref _storageLock,
                () => GetStorageOnce(environment));
        }

        private ICompressedStorage GetStorageOnce(IDictionary<string, object> environment)
        {
            var storage = _options.CompressedStorageProvider.Create();
            var onAppDisposing = new OwinRequest(environment).Get<CancellationToken>("host.OnAppDisposing");
            if (onAppDisposing != CancellationToken.None)
            {
                onAppDisposing.Register(storage.Dispose);
            }
            return storage;
        }

        private IEncoding SelectCompression(IDictionary<string, object> environment)
        {
            var request = new OwinRequest(environment);

            var bestAccept = new Accept { Encoding = "identity", Quality = 0 };
            IEncoding bestEncoding = null;

            var acceptEncoding = request.GetHeaderUnmodified("accept-encoding");
            if (acceptEncoding != null)
            {
                foreach (var segment in new HeaderSegments(acceptEncoding))
                {
                    if (!segment.Data.HasValue)
                    {
                        continue;
                    }
                    var accept = Parse(segment.Data.Value);
                    if (accept.Quality == 0 || accept.Quality < bestAccept.Quality)
                    {
                        continue;
                    }
                    var compression = _options.EncodingProvider.GetCompression(accept.Encoding);
                    if (compression == null)
                    {
                        continue;
                    }
                    bestAccept = accept;
                    bestEncoding = compression;
                    if (accept.Quality == 1000)
                    {
                        break;
                    }
                }
            }
            return bestEncoding;
        }

        struct Accept
        {
            public string Encoding;
            public int Quality;
        }

        private Accept Parse(string value)
        {
            var encoding = value;
            int quality = 1000;
            var detail = "";

            var semicolonIndex = value.IndexOf(';');
            if (semicolonIndex != -1)
            {
                encoding = value.Substring(0, semicolonIndex);
                detail = value.Substring(semicolonIndex + 1);
            }
            var qualityIndex = detail.IndexOf("q=", StringComparison.OrdinalIgnoreCase);
            if (qualityIndex != -1)
            {
                quality = (int)(double.Parse(detail.Substring(qualityIndex + 2)) * 1000 + .5);
            }
            return new Accept
            {
                Encoding = encoding.Trim(),
                Quality = quality
            };
        }
    }

    class StaticCompressionContext
    {
        private IDictionary<string, object> _environment;
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

        internal enum InterceptMode
        {
            Uninitialized,
            DoingNothing,
            CompressingToStorage,
            SentFromStorage,
        }

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

        struct Tacking
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

        public void Attach()
        {
            //TODO: remove encoding marks from etag-containing request headers
            //TODO: look to see if this is already added?
            _response.AddHeaderJoined("Vary", "Accept-Encoding");

            _originalIfNoneMatch = _request.GetHeaderUnmodified("If-None-Match");
            if (_originalIfNoneMatch != null)
            {
                var tacking = new Tacking();
                var modified = false;
                foreach (var segment in new HeaderSegments(_originalIfNoneMatch))
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
                    _request.SetHeader("If-None-Match", tacking.BuildString());
                }
                else
                {
                    _originalIfNoneMatch = null;
                }
            }
            //var originalIfNoneMatch = _request.GetHeaderUnmodified("If-None-Match");

            _originalResponseBody = _response.Body;
            _response.Body = new SwitchingStream(this, _originalResponseBody);
            _originalSendFileAsyncDelegate = _response.SendFileAsyncDelegate;
            _response.SendFileAsyncDelegate = SendFileASync;
        }

        public void Detach()
        {
            Intercept(detaching: true);
            _response.Body = _originalResponseBody;
            _response.SendFileAsyncDelegate = _originalSendFileAsyncDelegate;
            if (_originalIfNoneMatch != null)
            {
                _request.SetHeaderUnmodified("If-None-Match", _originalIfNoneMatch);
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

        static readonly Func<InterceptMode> InterceptDetaching = () => InterceptMode.DoingNothing;
        private string _compressedETag;
        private ICompressedItemHandle _compressedItem;
        private string[] _originalIfNoneMatch;

        private static readonly StringSegment CommaSegment = new StringSegment(", ", 0, 2);
        private static readonly StringSegment QuoteSegment = new StringSegment("\"", 0, 1);

        public InterceptMode InterceptOnce()
        {
            var etag = SingleSegment(_response, "ETag");

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

            var statusCode = _response.StatusCode;
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
            var cursor = new HeaderSegments(response.GetHeaderUnmodified(header)).GetEnumerator();
            if (cursor.MoveNext())
            {
                var segment = cursor.Current;
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
            var interceptMode = Intercept();
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
                        //TODO: stream copy operation
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
                            //TODO: stream copy operation
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

                        //TODO: sync errors go faulted task
                        var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        fileStream.Seek(offset, SeekOrigin.Begin);
                        var copyOperation = new StreamCopyOperation(fileStream, _originalResponseBody, count, cancel);
                        return copyOperation.Start().Finally(fileStream.Close);
                    }
                case InterceptMode.CompressingToStorage:
                    {
                        //TODO: sync errors go faulted task
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
    }
}
