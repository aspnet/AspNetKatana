// -----------------------------------------------------------------------
// <copyright file="StaticFiles.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin.StaticFiles.FileSystems;

// Notes: The larger Static Files feature includes several sub modules:
// - DefaultFile: If the given path is a directory, append a default file name (if it exists on disc).
// - BrowseDirs: If the given path is for a directory, list its contents
// - StaticFiles: This module; locate an individual file and serve it.
// - SendFileMiddleware: Insert a SendFile delegate if none is present
// - UploadFile: Supports receiving files (or modifying existing files).
namespace Microsoft.Owin.StaticFiles
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    using SendFileFunc = Func<string, long, long?, CancellationToken, Task>;

    public class StaticFileMiddleware
    {
        private readonly AppFunc _next;
        private readonly StaticFileOptions _options;

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public StaticFileMiddleware(AppFunc next, StaticFileOptions options)
        {
            _next = next;
            _options = options;
        }

        public Task Invoke(IDictionary<string, object> environment)
        {
            // Check if the URL matches any expected paths
            string subpath;
            string contentType;
            IFileInfo fileInfo;
            if (Helpers.IsGetOrHeadMethod(environment) &&
                TryMatchPath(environment, out subpath) &&
                TryGetContentType(subpath, out contentType) &&
                TryGetFileInfo(subpath, out fileInfo))
            {
                SendFileFunc sendFileAsync = GetSendFile(environment);
                Tuple<long, long?> range = SetHeaders(environment, contentType, fileInfo);
                if (Helpers.IsGetMethod(environment))
                {
                    return sendFileAsync(fileInfo.PhysicalPath, range.Item1, range.Item2, GetCancellationToken(environment));
                }
                else
                {
                    // HEAD, no response body
                    return Constants.CompletedTask;
                }
            }

            return _next(environment);
        }

        private bool TryMatchPath(IDictionary<string, object> environment, out string subpath)
        {
            string path = (string)environment[Constants.RequestPathKey];
            string matchUrl = _options.RequestPath;

            if (path.StartsWith(matchUrl, StringComparison.OrdinalIgnoreCase)
                && (path.Length == matchUrl.Length || path[matchUrl.Length] == '/'))
            {
                subpath = path.Substring(matchUrl.Length);
                return true;
            }
            subpath = null;
            return false;
        }

        private bool TryGetContentType(string subpath, out string contentType)
        {
            if (_options.ContentTypeProvider.TryGetContentType(subpath, out contentType))
            {
                return true;
            }

            if (!string.IsNullOrEmpty(_options.DefaultContentType))
            {
                contentType = _options.DefaultContentType;
                return true;
            }

            return false;
        }

        private bool TryGetFileInfo(string subpath, out IFileInfo file)
        {
            return _options.FileSystemProvider.TryGetFileInfo(subpath, out file);
        }

        private static SendFileFunc GetSendFile(IDictionary<string, object> environment)
        {
            object obj;
            if (environment.TryGetValue(Constants.SendFileAsyncKey, out obj))
            {
                SendFileFunc func = obj as SendFileFunc;
                if (func != null)
                {
                    return func;
                }
            }

            throw new MissingMethodException(string.Empty, "SendFileFunc");
        }

        // Set response headers:
        // Content-Length/chunked
        // Content-Type
        // TODO: Ranges
        private static Tuple<long, long?> SetHeaders(IDictionary<string, object> environment, string contentType, IFileInfo fileInfo)
        {
            var responseHeaders = (IDictionary<string, string[]>)environment[Constants.ResponseHeadersKey];

            long length = fileInfo.Length;
            // responseHeaders["Transfer-Encoding"] = new[] { "chunked" };
            responseHeaders[Constants.ContentLength] = new[] { length.ToString(CultureInfo.InvariantCulture) };
            responseHeaders[Constants.ContentType] = new[] { contentType };

            return new Tuple<long, long?>(0, length);
        }

        private static CancellationToken GetCancellationToken(IDictionary<string, object> environment)
        {
            return (CancellationToken)environment[Constants.CallCancelledKey];
        }
    }
}
