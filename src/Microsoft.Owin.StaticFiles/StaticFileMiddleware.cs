// <copyright file="StaticFileMiddleware.cs" company="Katana contributors">
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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin.StaticFiles.FileSystems;
using Owin.Types;

namespace Microsoft.Owin.StaticFiles
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    using SendFileFunc = Func<string, long, long?, CancellationToken, Task>;

    /// <summary>
    /// Enables serving static files for a given request path
    /// </summary>
    public class StaticFileMiddleware
    {
        private readonly AppFunc _next;
        private readonly StaticFileOptions _options;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="next"></param>
        /// <param name="options"></param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public StaticFileMiddleware(AppFunc next, StaticFileOptions options)
        {
            _next = next;
            _options = options;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        /// <returns></returns>
        public Task Invoke(IDictionary<string, object> environment)
        {
            // Check if the URL matches any expected paths
            var context = new StaticFileContext(environment, _options);
            if (context.ValidateMethod()
                && context.ValidatePath()
                && context.LookupContentType()
                && context.LookupFileInfo())
            {
                context.ComprehendRequestHeaders();
                context.ApplyResponseHeaders();

                var preconditionState = context.GetPreconditionState();
                if (preconditionState == StaticFileContext.PreconditionState.NotModified)
                {
                    return context.SendStatusAsync(304);
                }
                if (preconditionState == StaticFileContext.PreconditionState.PreconditionFailed)
                {
                    return context.SendStatusAsync(412);
                }
                if (context.IsHeadMethod)
                {
                    return context.SendStatusAsync(200);
                }
                return context.SendAsync(200);
            }

            return _next(environment);
        }
    }

    struct StaticFileContext
    {
        private readonly IDictionary<string, object> _environment;
        private readonly StaticFileOptions _options;
        private OwinRequest _request;
        private OwinResponse _response;
        private string _method;
        private bool _isGet;
        private bool _isHead;
        private string _subPath;
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

        internal enum PreconditionState
        {
            Unspecified,
            NotModified,
            ShouldProcess,
            PreconditionFailed
        }

        public StaticFileContext(IDictionary<string, object> environment, StaticFileOptions options)
        {
            _environment = environment;
            _options = options;
            _request = new OwinRequest(environment);
            _response = new OwinResponse(environment);

            _method = null;
            _isGet = false;
            _isHead = false;
            _subPath = null;
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

        public bool IsHeadMethod { get { return _isHead; } }

        public bool ValidateMethod()
        {
            _method = _request.Method;
            _isGet = string.Equals(_method, "GET", StringComparison.OrdinalIgnoreCase);
            _isHead = string.Equals(_method, "HEAD", StringComparison.OrdinalIgnoreCase);
            return _isGet || _isHead;
        }

        public bool ValidatePath()
        {
            return Helpers.TryMatchPath(_environment, _options.RequestPath, forDirectory: false, subpath: out _subPath);
        }

        public bool LookupContentType()
        {
            if (_options.ContentTypeProvider.TryGetContentType(_subPath, out _contentType))
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
            var found = _options.FileSystemProvider.TryGetFileInfo(_subPath, out _fileInfo);
            if (found)
            {
                _length = _fileInfo.Length;
                _lastModified = _fileInfo.LastModified;
                _lastModifiedString = _lastModified.ToString("r", CultureInfo.InvariantCulture);

                _etag = '\"' + _lastModified.ToFileTimeUtc().ToString(CultureInfo.InvariantCulture) + '\"';
            }
            return found;
        }

        public void ComprehendRequestHeaders()
        {
            var etag = _etag;

            var ifMatch = _request.GetHeaderSplit("If-Match");
            if (ifMatch != null)
            {
                var matches = ifMatch.Any(value => string.Equals(value, etag, StringComparison.OrdinalIgnoreCase));
                _ifMatchState = matches ? PreconditionState.ShouldProcess : PreconditionState.PreconditionFailed;
            }

            var ifNoneMatch = _request.GetHeaderSplit("If-None-Match");
            if (ifNoneMatch != null)
            {
                var matches = ifNoneMatch.Any(value => string.Equals(value, etag, StringComparison.OrdinalIgnoreCase));
                _ifNoneMatchState = matches ? PreconditionState.NotModified : PreconditionState.ShouldProcess;
            }

            var ifModifiedSince = _request.GetHeader("If-Modified-Since");
            if (ifModifiedSince != null)
            {
                var matches = string.Equals(ifModifiedSince, _lastModifiedString, StringComparison.Ordinal);
                _ifModifiedSinceState = matches ? PreconditionState.NotModified : PreconditionState.ShouldProcess;
            }

            var ifUnmodifiedSince = _request.GetHeader("If-Unmodified-Since");
            if (ifUnmodifiedSince != null)
            {
                var matches = string.Equals(ifModifiedSince, _lastModifiedString, StringComparison.Ordinal);
                _ifUnmodifiedSinceState = matches ? PreconditionState.ShouldProcess : PreconditionState.PreconditionFailed;
            }
        }

        public void ApplyResponseHeaders()
        {
            _response.SetHeader(Constants.ContentLength, _length.ToString(CultureInfo.InvariantCulture));

            if (!string.IsNullOrEmpty(_contentType))
            {
                _response.SetHeader(Constants.ContentType, _contentType);
            }

            _response.SetHeader("Last-Modified", _lastModifiedString);
            _response.SetHeader("ETag", _etag);
        }

        public PreconditionState GetPreconditionState()
        {
            var matchState = _ifMatchState > _ifNoneMatchState ? _ifMatchState : _ifNoneMatchState;
            var modifiedState = _ifModifiedSinceState > _ifUnmodifiedSinceState ? _ifModifiedSinceState : _ifUnmodifiedSinceState;
            return matchState > modifiedState ? matchState : modifiedState;
        }

        public Task SendStatusAsync(int statusCode)
        {
            _response.StatusCode = statusCode;
            return Constants.CompletedTask;
        }

        public Task SendAsync(int statusCode)
        {
            _response.StatusCode = statusCode;
            var physicalPath = _fileInfo.PhysicalPath;
            if (_response.CanSendFile && !string.IsNullOrEmpty(physicalPath))
            {
                return _response.SendFileAsync(physicalPath, 0, _length, _request.CallCancelled);
            }

            var readStream = _fileInfo.CreateReadStream();
            var copyOperation = new StreamCopyOperation(readStream, _response.Body, _length, _request.CallCancelled);
            var task = copyOperation.Start();
            task.ContinueWith(resultTask => readStream.Close(), TaskContinuationOptions.ExecuteSynchronously);
            return task;
        }

    }
}
