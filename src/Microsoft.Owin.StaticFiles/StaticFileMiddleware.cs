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
            string subpath;
            string contentType;
            IFileInfo fileInfo;
            if (Helpers.IsGetOrHeadMethod(environment)
                && Helpers.TryMatchPath(environment, _options.RequestPath, forDirectory: false, subpath: out subpath)
                && TryGetContentType(subpath, out contentType)
                && TryGetFileInfo(subpath, out fileInfo))
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
