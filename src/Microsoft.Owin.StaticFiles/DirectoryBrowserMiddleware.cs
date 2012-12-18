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
                    SetRedirect(environment);                    
                    return Constants.CompletedTask;
                }

                StringBuilder builder = GenerateContent(environment, directory);
                if (Helpers.IsGetMethod(environment))
                {
                    return SendContentAsync(environment, builder);
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
        private static void SetRedirect(IDictionary<string, object> environment)
        {
            environment[Constants.ResponseStatusCodeKey] = 301;
            var responseHeaders = (IDictionary<string, string[]>)environment[Constants.ResponseHeadersKey];
            string basePath = (string)environment[Constants.RequestBasePathKey];
            string path = (string)environment[Constants.RequestPathKey];

            responseHeaders[Constants.Location] = new string[] { basePath + path + "/" };
        }

        private StringBuilder GenerateContent(IDictionary<string, object> environment, IDirectoryInfo directoryInfo)
        {
            StringBuilder builder;

            // 1) Detect the requested content-type
            string contentType = DetermineContentType(environment);

            // 2) Generate the list of files and directories according to that type
            if (contentType == "text/plain")
            {
                builder = GenerateContentPlainText(directoryInfo);
            }
            else
            {
                throw new NotImplementedException(contentType);
            }

            SetHeaders(environment, builder, contentType);

            return builder;
        }

        // Reads the accept header and selects the most appropriate supported content-type
        private string DetermineContentType(IDictionary<string, object> environment)
        {
            // TODO:
            // Parse the Accept header

            // Check for supported types:
            // text/plain
            // text/html
            // application/json
            // text/xml or application/xml?

            return "text/plain";
        }

        private static StringBuilder GenerateContentPlainText(IDirectoryInfo directoryInfo)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("/{0}/\r\n", directoryInfo.Name);

            foreach (IDirectoryInfo subdir in directoryInfo.GetDirectories())
            {
                builder.AppendFormat("/{0}/\r\n", subdir.Name);
            }
            foreach (IFileInfo file in directoryInfo.GetFiles())
            {
                builder.AppendFormat("{0}, {1}, {2}\r\n", file.Name, file.Length, file.LastModified);
            }
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
