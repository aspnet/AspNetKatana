// -----------------------------------------------------------------------
// <copyright file="DirectoryBrowser.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin.StaticFiles.FileSystems;

namespace Microsoft.Owin.StaticFiles
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class DirectoryBrowserMiddleware
    {
        private StaticFileOptions _options;
        private AppFunc _next;

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public DirectoryBrowserMiddleware(AppFunc next, StaticFileOptions options)
        {
            _options = options;
            _next = next;
        }

        public Task Invoke(IDictionary<string, object> environment)
        {
            // Check if the URL matches any expected paths
            string subpath;
            IDirectoryInfo directory;
            if (Helpers.IsGetOrHeadMethod(environment) 
                && TryMatchPath(environment, out subpath)
                && TryGetDirectoryInfo(subpath, out directory))
            {
                if (!PathEndsInSlash(environment))
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

        private bool TryMatchPath(IDictionary<string, object> environment, out string subpath)
        {
            string path = (string)environment[Constants.RequestPathKey];
            string matchUrl = _options.RequestPath;

            if (path.Length == 0 || path[path.Length - 1] != '/')
            {
                path += "/";
            }

            if (path.StartsWith(matchUrl, StringComparison.OrdinalIgnoreCase)
                && (path.Length == matchUrl.Length || path[matchUrl.Length] == '/'))
            {
                subpath = path.Substring(matchUrl.Length);
                return true;
            }
            subpath = null;
            return false;
        }

        private bool TryGetDirectoryInfo(string subpath, out IDirectoryInfo directory)
        {
            return _options.FileSystemProvider.TryGetDirectoryInfo(subpath, out directory);
        }

        private static bool PathEndsInSlash(IDictionary<string, object> environment)
        {
            string path = (string)environment[Constants.RequestPathKey];
            return path.EndsWith("/", StringComparison.Ordinal);
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

        private static bool TryGenerateContent(IDictionary<string, object> environment, IDirectoryInfo directoryInfo, out StringBuilder body)
        {
            // 1) Detect the requested content-type
            string contentType;
            if (!TryDetermineContentType(environment, out contentType))
            {
                body = null;
                return false;
            }

            string requestPath = (string)environment[Constants.RequestPathBaseKey]
                + (string)environment[Constants.RequestPathKey];

            // 2) Generate the list of files and directories according to that type
            if (contentType.Equals(Constants.TextPlain, StringComparison.OrdinalIgnoreCase))
            {
                body = GenerateContentPlainText(requestPath, directoryInfo);
            }
            else if (contentType.Equals(Constants.TextHtml, StringComparison.OrdinalIgnoreCase))
            {
                body = GenerateContentHtml(requestPath, directoryInfo);
            }
            else
            {
                throw new NotImplementedException(contentType);
            }

            SetHeaders(environment, body, contentType);

            return true;
        }

        // Reads the accept header and selects the most appropriate supported content-type
        private static bool TryDetermineContentType(IDictionary<string, object> environment, out string contentType)
        {
            // TODO:
            // Parse the Accept header
            var requestHeaders = (IDictionary<string, string[]>)environment[Constants.RequestHeadersKey];

            string[] acceptHeaders;
            if (!requestHeaders.TryGetValue(Constants.Accept, out acceptHeaders)
                || acceptHeaders == null || acceptHeaders.Length == 0)
            {
                // RFC 2616 section 14.1: If no Accept header is present, then it is assumed that the client accepts all media types.
                // Send our fanciest version.
                contentType = Constants.TextHtml;
                return true;
            }

            string acceptHeader = acceptHeaders[0] ?? string.Empty;
            if (acceptHeaders.Length > 1)
            {
                acceptHeader = string.Join(", ", acceptHeaders);
            }

            // TODO: Split by "," and sort by q value and our priorities
            // acceptHeaders = acceptHeader.Split(",");

            // Check for supported types:
            // text/plain
            // text/html
            // application/json
            // text/xml or application/xml?
            // */*?
            // text/*?
            string[] supportedTypes = new string[] { Constants.TextHtml, Constants.TextPlain };
            for (int i = 0; i < supportedTypes.Length; i++)
            {
                if (acceptHeader.Contains(supportedTypes[i]))
                {
                    contentType = supportedTypes[i];
                    return true;
                }
            }

            contentType = null;
            return false;
        }

        private static StringBuilder GenerateContentPlainText(string requestPath, IDirectoryInfo directoryInfo)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("{0}\r\n", requestPath);
            builder.Append("\r\n");

            foreach (IDirectoryInfo subdir in directoryInfo.GetDirectories())
            {
                builder.AppendFormat("{0}/\r\n", subdir.Name);
            }
            builder.Append("\r\n");

            foreach (IFileInfo file in directoryInfo.GetFiles())
            {
                builder.AppendFormat("{0}, {1}, {2}\r\n", file.Name, file.Length, file.LastModified);
            }
            builder.Append("\r\n");

            return builder;
        }

        private static StringBuilder GenerateContentHtml(string requestPath, IDirectoryInfo directoryInfo)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("<html><body>");

            builder.AppendFormat("<h1>{0}</h1>", requestPath);
            builder.Append("<br>");

            foreach (IDirectoryInfo subdir in directoryInfo.GetDirectories())
            {
                builder.AppendFormat("<a href=\"./{0}/\">{0}/</a><br>", subdir.Name);
            }
            builder.Append("<br>");

            foreach (IFileInfo file in directoryInfo.GetFiles())
            {
                builder.AppendFormat("<a href=\"./{0}\">{0}</a>, {1}, {2}<br>", file.Name, file.Length, file.LastModified);
            }
            builder.Append("<br>");

            builder.Append("</body></html>");
            return builder;
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
