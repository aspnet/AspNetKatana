// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin.FileSystems;

namespace Microsoft.Owin.StaticFiles
{
    using SendFileFunc = Func<string, long, long?, CancellationToken, Task>;

    internal struct StaticFileContext
    {
        private readonly IOwinContext _context;
        private readonly StaticFileOptions _options;
        private readonly PathString _matchUrl;
        private readonly IOwinRequest _request;
        private readonly IOwinResponse _response;
        private string _method;
        private bool _isGet;
        private bool _isHead;
        private PathString _subPath;
        private string _contentType;
        private IFileInfo _fileInfo;
        private long _length;
        private DateTime _lastModified;
        private string _lastModifiedString;
        private string _etag;

        private PreconditionState _ifMatchState;
        private PreconditionState _ifNoneMatchState;
        private PreconditionState _ifModifiedSinceState;
        private PreconditionState _ifUnmodifiedSinceState;

        public StaticFileContext(IOwinContext context, StaticFileOptions options, PathString matchUrl)
        {
            _context = context;
            _options = options;
            _matchUrl = matchUrl;
            _request = context.Request;
            _response = context.Response;

            _method = null;
            _isGet = false;
            _isHead = false;
            _contentType = null;
            _fileInfo = null;
            _length = 0;
            _lastModified = new DateTime();
            _etag = null;
            _lastModifiedString = null;
            _ifMatchState = PreconditionState.Unspecified;
            _ifNoneMatchState = PreconditionState.Unspecified;
            _ifModifiedSinceState = PreconditionState.Unspecified;
            _ifUnmodifiedSinceState = PreconditionState.Unspecified;
        }

        internal enum PreconditionState
        {
            Unspecified,
            NotModified,
            ShouldProcess,
            PreconditionFailed
        }

        public bool IsHeadMethod
        {
            get { return _isHead; }
        }

        public bool ValidateMethod()
        {
            _method = _request.Method;
            _isGet = Helpers.IsGetMethod(_method);
            _isHead = Helpers.IsHeadMethod(_method);
            return _isGet || _isHead;
        }

        // Check if the URL matches any expected paths
        public bool ValidatePath()
        {
            return Helpers.TryMatchPath(_context, _matchUrl, forDirectory: false, subpath: out _subPath);
        }

        public bool LookupContentType()
        {
            if (_options.ContentTypeProvider.TryGetContentType(_subPath.Value, out _contentType))
            {
                return true;
            }

            if (_options.ServeUnknownFileTypes)
            {
                _contentType = _options.DefaultContentType;
                return true;
            }

            return false;
        }

        public bool LookupFileInfo()
        {
            bool found = _options.FileSystem.TryGetFileInfo(_subPath.Value, out _fileInfo);
            if (found)
            {
                _length = _fileInfo.Length;

                DateTime last = _fileInfo.LastModified;
                // Truncate to the second.
                _lastModified = new DateTime(last.Year, last.Month, last.Day, last.Hour, last.Minute, last.Second, last.Kind);
                _lastModifiedString = _lastModified.ToString("r", CultureInfo.InvariantCulture);

                long etagHash = _lastModified.ToFileTimeUtc() ^ _length;
                _etag = Convert.ToString(etagHash, 16);
            }
            return found;
        }

        public void ComprehendRequestHeaders()
        {
            // TODO: Range requests
            string etag = _etag;

            // 14.24 If-Match
            IList<string> ifMatch = _request.Headers.GetCommaSeparatedValues("If-Match"); // Removes quotes
            if (ifMatch != null)
            {
                _ifMatchState = PreconditionState.PreconditionFailed;
                foreach (var segment in ifMatch)
                {
                    if (segment.Equals("*", StringComparison.Ordinal)
                        || segment.Equals(etag, StringComparison.Ordinal))
                    {
                        _ifMatchState = PreconditionState.ShouldProcess;
                        break;
                    }
                }
            }

            // 14.26 If-None-Match
            IList<string> ifNoneMatch = _request.Headers.GetCommaSeparatedValues("If-None-Match");
            if (ifNoneMatch != null)
            {
                _ifNoneMatchState = PreconditionState.ShouldProcess;
                foreach (var segment in ifNoneMatch)
                {
                    if (segment.Equals("*", StringComparison.Ordinal)
                        || segment.Equals(etag, StringComparison.Ordinal))
                    {
                        _ifNoneMatchState = PreconditionState.NotModified;
                        break;
                    }
                }
            }

            // 14.25 If-Modified-Since
            string ifModifiedSinceString = _request.Headers.Get("If-Modified-Since");
            DateTime ifModifiedSince;
            if (DateTime.TryParseExact(ifModifiedSinceString, "r", CultureInfo.InvariantCulture, DateTimeStyles.None, out ifModifiedSince))
            {
                bool modified = ifModifiedSince < _lastModified;
                _ifModifiedSinceState = modified ? PreconditionState.ShouldProcess : PreconditionState.NotModified;
            }

            // 14.28 If-Unmodified-Since
            string ifUnmodifiedSinceString = _request.Headers.Get("If-Unmodified-Since");
            DateTime ifUnmodifiedSince;
            if (DateTime.TryParseExact(ifUnmodifiedSinceString, "r", CultureInfo.InvariantCulture, DateTimeStyles.None, out ifUnmodifiedSince))
            {
                bool unmodified = ifUnmodifiedSince >= _lastModified;
                _ifUnmodifiedSinceState = unmodified ? PreconditionState.ShouldProcess : PreconditionState.PreconditionFailed;
            }
        }

        public void ApplyResponseHeaders()
        {
            if (!string.IsNullOrEmpty(_contentType))
            {
                _response.ContentType = _contentType;
            }

            _response.Headers.Set("Last-Modified", _lastModifiedString);
            _response.ETag = '\"' + _etag + "\"";
        }

        public PreconditionState GetPreconditionState()
        {
            PreconditionState matchState = _ifMatchState > _ifNoneMatchState ? _ifMatchState : _ifNoneMatchState;
            PreconditionState modifiedState = _ifModifiedSinceState > _ifUnmodifiedSinceState ? _ifModifiedSinceState : _ifUnmodifiedSinceState;
            return matchState > modifiedState ? matchState : modifiedState;
        }

        public Task SendStatusAsync(int statusCode)
        {
            _response.StatusCode = statusCode;
            if (statusCode == Constants.Status200Ok)
            {
                _response.ContentLength = _length;
            }
            return Constants.CompletedTask;
        }

        public Task SendAsync(int statusCode)
        {
            _response.StatusCode = statusCode;
            _response.ContentLength = _length;

            string physicalPath = _fileInfo.PhysicalPath;
            SendFileFunc sendFile = _response.Get<SendFileFunc>("sendfile.SendAsync");
            if (sendFile != null && !string.IsNullOrEmpty(physicalPath))
            {
                return sendFile(physicalPath, 0, _length, _request.CallCancelled);
            }

            Stream readStream = _fileInfo.CreateReadStream();
            var copyOperation = new StreamCopyOperation(readStream, _response.Body, _length, _request.CallCancelled);
            Task task = copyOperation.Start();
            task.ContinueWith(resultTask => readStream.Close(), TaskContinuationOptions.ExecuteSynchronously);
            return task;
        }
    }
}
