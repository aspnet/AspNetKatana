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
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Owin.StaticFiles
{
    using System.Diagnostics.Contracts;
    using System.Net;
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class DirectoryBrowser
    {
        private IList<KeyValuePair<string, string>> _pathsAndDirectories;
        private AppFunc _next;

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public DirectoryBrowser(AppFunc next, IList<KeyValuePair<string, string>> pathsAndDirectories)
        {
            _pathsAndDirectories = pathsAndDirectories;
            _next = next;
        }

        public Task Invoke(IDictionary<string, object> environment)
        {
            // Check if the URL matches any expected paths
            string directory;
            bool redirectAddSlash;
            if (Helpers.IsGetOrHeadMethod(environment) && TryMatchDirectory(environment, out directory, out redirectAddSlash))
            {
                if (redirectAddSlash)
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

        private bool TryMatchDirectory(IDictionary<string, object> environment, out string directory, out bool redirectAddSlash)
        {
            redirectAddSlash = false;
            string path = (string)environment[Constants.RequestPathKey];
            if (!path.EndsWith("/", StringComparison.Ordinal))
            {
                path += "/";
                redirectAddSlash = true;
            }

            for (int i = 0; i < _pathsAndDirectories.Count; i++)
            {
                KeyValuePair<string, string> pair = _pathsAndDirectories[i];
                string matchUrl = pair.Key;
                string matchDir = pair.Value;

                // Only full path segment matches are allowed; e.g. request for /foo/ matches /foo/,
                // or /bar/foo/ matches /bar/
                if (matchUrl != null && matchUrl.Length > 0 && matchUrl[matchUrl.Length - 1] == '/')
                {
                    if (path.StartsWith(matchUrl, StringComparison.OrdinalIgnoreCase))
                    {
                        string subpath = path.Substring(matchUrl.Length);
                        directory = matchDir + subpath.Replace('/', '\\');

                        if (Directory.Exists(directory))
                        {
                            return true;
                        }
                    }
                }
            }

            directory = null;
            return false;
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

        private StringBuilder GenerateContent(IDictionary<string, object> environment, string directory)
        {
            DirectoryInfo info = new DirectoryInfo(directory);
            Contract.Assert(info.Exists);
            StringBuilder builder;

            // 1) Detect the requested content-type
            string contentType = DetermineContentType(environment);

            // 2) Generate the list of files and directories according to that type
            if (contentType == "text/plain")
            {
                builder = GenerateContentPlainText(info);
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

        private static StringBuilder GenerateContentPlainText(DirectoryInfo info)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("/{0}/\r\n", info.Name);

            foreach (DirectoryInfo subdir in info.GetDirectories())
            {
                builder.AppendFormat("/{0}/\r\n", subdir.Name);
            }
            foreach (FileInfo file in info.GetFiles())
            {
                builder.AppendFormat("{0}, {1}, {2}\r\n", file.Name, file.Length, file.LastWriteTime);
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
