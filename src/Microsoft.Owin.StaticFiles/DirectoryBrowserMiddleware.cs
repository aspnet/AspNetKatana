// -----------------------------------------------------------------------
// <copyright file="DirectoryBrowser.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.StaticFiles.ContentTypes;
using Microsoft.Owin.StaticFiles.DirectoryFormatters;
using Microsoft.Owin.StaticFiles.FileSystems;

namespace Microsoft.Owin.StaticFiles
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    /// <summary>
    /// Enables directory browsing
    /// </summary>
    public class DirectoryBrowserMiddleware
    {
        private readonly DirectoryBrowserOptions _options;
        private readonly AppFunc _next;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="next"></param>
        /// <param name="options"></param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public DirectoryBrowserMiddleware(AppFunc next, DirectoryBrowserOptions options)
        {
            _options = options;
            _next = next;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="environment"></param>
        /// <returns></returns>
        public Task Invoke(IDictionary<string, object> environment)
        {
            if (environment == null)
            {
                throw new ArgumentNullException("environment");
            }

            // Check if the URL matches any expected paths
            string subpath;
            IDirectoryInfo directory;
            if (Helpers.IsGetOrHeadMethod(environment)
                && Helpers.TryMatchPath(environment, _options.RequestPath, forDirectory: true, subpath: out subpath)
                && TryGetDirectoryInfo(subpath, out directory))
            {
                if (!Helpers.PathEndsInSlash(environment))
                {
                    RedirectToAddSlash(environment);
                    return Constants.CompletedTask;
                }

                StringBuilder body;
                if (!TryGenerateContent(environment, directory, out body))
                {
                    // 406: Not Acceptable, we couldn't generate the requested content-type.
                    environment[Constants.ResponseStatusCodeKey] = 406;
                    return Constants.CompletedTask;
                }

                if (Helpers.IsGetMethod(environment))
                {
                    return SendContentAsync(environment, body);
                }
                else
                {
                    // HEAD, no response body
                    return Constants.CompletedTask;
                }
            }

            return _next(environment);
        }

        private bool TryGetDirectoryInfo(string subpath, out IDirectoryInfo directory)
        {
            return _options.FileSystemProvider.TryGetDirectoryInfo(subpath, out directory);
        }
        
        // Redirect to append a slash to the path
        private static void RedirectToAddSlash(IDictionary<string, object> environment)
        {
            environment[Constants.ResponseStatusCodeKey] = 301;
            var responseHeaders = (IDictionary<string, string[]>)environment[Constants.ResponseHeadersKey];
            string basePath = (string)environment[Constants.RequestPathBaseKey];
            string path = (string)environment[Constants.RequestPathKey];

            responseHeaders[Constants.Location] = new string[] { basePath + path + "/" };
        }

        private bool TryGenerateContent(IDictionary<string, object> environment, IDirectoryInfo directoryInfo, out StringBuilder body)
        {
            // 1) Detect the requested content-type
            IDirectoryInfoFormatter formatter;
            if (!_options.FormatSelector.TryDetermineFormatter(environment, out formatter))
            {
                body = null;
                return false;
            }

            string requestPath = (string)environment[Constants.RequestPathBaseKey]
                + (string)environment[Constants.RequestPathKey];

            // 2) Generate the list of files and directories according to that type
            body = formatter.GenerateContent(requestPath, directoryInfo);

            SetHeaders(environment, body, formatter.ContentType);

            return true;
        }

        private static void SetHeaders(IDictionary<string, object> environment, StringBuilder builder, string contentType)
        {
            var responseHeaders = (IDictionary<string, string[]>)environment[Constants.ResponseHeadersKey];

            long length = builder.Length;
            // responseHeaders["Transfer-Encoding"] = new[] { "chunked" };
            responseHeaders[Constants.ContentLength] = new[] { length.ToString(CultureInfo.InvariantCulture) };
            responseHeaders[Constants.ContentType] = new[] { contentType };
        }

        // TODO: Encoding?
        private static Task SendContentAsync(IDictionary<string, object> environment, StringBuilder builder)
        {
            var responseBody = (Stream)environment[Constants.ResponseBodyKey];
            byte[] body = Encoding.ASCII.GetBytes(builder.ToString());
            return responseBody.WriteAsync(body, 0, body.Length, Helpers.GetCancellationToken(environment));
        }
    }
}
