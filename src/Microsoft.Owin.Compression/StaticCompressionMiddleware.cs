using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin.Compression.Encoding;
using Microsoft.Owin.Compression.Infrastructure;
using Microsoft.Owin.Compression.Storage;
using Owin.Types;

namespace Microsoft.Owin.Compression
{
    public class StaticCompressionMiddleware
    {
        private readonly Func<IDictionary<string, object>, Task> _next;
        private readonly StaticCompressionOptions _options;

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

            var context = new StaticCompressionContext(environment, _options, compression);
            context.Attach();
            return _next(environment)
                .Then((Func<Task>)context.Complete)
                .Catch(context.Complete);
        }

        private ICompression SelectCompression(IDictionary<string, object> environment)
        {
            var request = new OwinRequest(environment);

            var bestAccept = new Accept { Encoding = "identity", Quality = 0 };
            ICompression bestCompression = null;

            foreach (var value in request.GetHeaderSplit("accept-encoding"))
            {
                var accept = Parse(value);
                if (accept.Quality == 0 || accept.Quality < bestAccept.Quality)
                {
                    continue;
                }
                var compression = _options.CompressionProvider.GetCompression(accept.Encoding);
                if (compression == null)
                {
                    continue;
                }
                bestAccept = accept;
                bestCompression = compression;
                if (accept.Quality == 1000)
                {
                    break;
                }
            }
            return bestCompression;
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
        private readonly ICompression _compression;
        private OwinRequest _request;
        private OwinResponse _response;
        private Stream _originalResponseBody;
        private Func<string, long, long?, CancellationToken, Task> _originalSendFileAsyncDelegate;
        private InterceptMode _intercept;
        private bool _interceptResponse;
        private bool _interceptInitialized;
        private object _interceptLock = new object();
        private Stream _compressingStream;
        private ICompressedEntryBuilder _compressedEntryBuilder;

        internal enum InterceptMode
        {
            Uninitialized,
            DoingNothing,
            CompressingToStorage,
            SentFromStorage,
        }

        public StaticCompressionContext(IDictionary<string, object> environment, StaticCompressionOptions options, ICompression compression)
        {
            _environment = environment;
            _options = options;
            _compression = compression;
            _request = new OwinRequest(environment);
            _response = new OwinResponse(environment);
        }

        public void Attach()
        {
            //TODO: look to see if this is already added?
            _response.AddHeaderJoined("Vary", "Accept-Encoding");

            _originalResponseBody = _response.Body;
            _response.Body = new SwitchingStream(this, _originalResponseBody);
            _originalSendFileAsyncDelegate = _response.SendFileAsyncDelegate;
            _response.SendFileAsyncDelegate = SendFileASync;
        }

        public void Detach()
        {
            _response.Body = _originalResponseBody;
            _response.SendFileAsyncDelegate = _originalSendFileAsyncDelegate;
        }

        public InterceptMode Intercept()
        {
            return LazyInitializer.EnsureInitialized(
                ref _intercept,
                ref _interceptInitialized,
                ref _interceptLock,
                InterceptOnce);
        }

        public InterceptMode InterceptOnce()
        {
            var contentLengthString = _response.GetHeader("Content-Length");
            var etag = _response.GetHeader("ETag");
            long contentLength;
            if (contentLengthString == null
                || etag == null
                || !long.TryParse(contentLengthString, out contentLength))
            {
                return InterceptMode.DoingNothing;
            }

            var key = new CompressedKey
            {
                Compression = _compression.Name,
                ContentLength = contentLength,
                ETag = etag,
                RequestPath = _request.Path,
                RequestQueryString = _request.QueryString,
                RequestMethod = _request.Method,
            };

            var compressedEntry = _options.CompressedStorage.Lookup(key);
            if (compressedEntry != null)
            {
                return InterceptMode.SentFromStorage;
            }

            _compressedEntryBuilder = _options.CompressedStorage.Start(key);
            _compressingStream = _compression.CompressTo(_compressedEntryBuilder.Stream);
            return InterceptMode.CompressingToStorage;
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
            Detach();
            return TaskHelpers.Completed();
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
                    if (_originalSendFileAsyncDelegate != null)
                    {
                        return _originalSendFileAsyncDelegate.Invoke(fileName, offset, count, cancel);
                    }

                    //TODO: open and xmit file as fallback
                    throw new NotImplementedException();
                    break;
                case InterceptMode.CompressingToStorage:
                    //TODO: open and xmit file to _targetStream
                    break;
            }
            return TaskHelpers.Completed();
        }

    }
}
