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

// Notes: The larger Static Files feature includes several sub modules:
// - DefaultFile: If the given path is a directory, append a default file name (if it exists on disc).
// - BrowseDirs: If the given path is for a directory, list its contents
// - StaticFiles: This module; locate an individual file and serve it.
// - SendFileFallback: Insert a SendFile delegate if none is present
// - UploadFile: Supports receiving files (or modifying existing files).
namespace Microsoft.Owin.StaticFiles
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    using SendFileFunc = Func<string, long, long?, CancellationToken, Task>;

    public class FileLookup
    {
        private const string DefaultContentType = "application/octet-stream";

        private IList<KeyValuePair<string, string>> _pathsAndDirectories;
        private AppFunc _next;

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        public FileLookup(AppFunc next, IList<KeyValuePair<string, string>> pathsAndDirectories)
        {
            _pathsAndDirectories = pathsAndDirectories;
            _next = next;
        }

        public Task Invoke(IDictionary<string, object> environment)
        {
            // Check if the URL matches any expected paths
            string file;
            if (IsGetOrHeadMethod(environment) && TryMatchFile(environment, out file))
            {
                SendFileFunc sendFileAsync = GetSendFile(environment);
                Tuple<long, long?> range = SetHeaders(environment, file);
                if (IsGetMethod(environment))
                {
                    return sendFileAsync(file, range.Item1, range.Item2, GetCancellationToken(environment));
                }
                else
                {
                    // HEAD, no response body
                    return Constants.CompletedTask;
                }
            }

            return _next(environment);
        }

        private static bool IsGetOrHeadMethod(IDictionary<string, object> environment)
        {
            string method = (string)environment[Constants.RequestMethod];
            return "GET".Equals(method, StringComparison.OrdinalIgnoreCase)
                || "HEAD".Equals(method, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsGetMethod(IDictionary<string, object> environment)
        {
            string method = (string)environment[Constants.RequestMethod];
            return "GET".Equals(method, StringComparison.OrdinalIgnoreCase);
        }

        private bool TryMatchFile(IDictionary<string, object> environment, out string file)
        {
            string path = (string)environment[Constants.RequestPathKey];

            for (int i = 0; i < _pathsAndDirectories.Count; i++)
            {
                KeyValuePair<string, string> pair = _pathsAndDirectories[i];
                string matchUrl = pair.Key;
                string matchDir = pair.Value;

                // Only full path segment matches are allowed; e.g. request for /foo/bar.txt matches /foo/
                // or bar.txt matches bar.txt
                if (path.StartsWith(matchUrl, StringComparison.OrdinalIgnoreCase)
                    && (path.Length == matchUrl.Length 
                        || matchUrl[matchUrl.Length - 1] == '/'))
                {
                    string subpath = path.Substring(matchUrl.Length);
                    file = matchDir + subpath.Replace('/', '\\');

                    if (File.Exists(file))
                    {
                        return true;
                    }
                }
            }

            file = null;
            return false;
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
        private static Tuple<long, long?> SetHeaders(IDictionary<string, object> environment, string file)
        {
            var responseHeaders = (IDictionary<string, string[]>)environment[Constants.ResponseHeadersKey];

            FileInfo fileInfo = new FileInfo(file);
            long length = fileInfo.Length;
            // responseHeaders["Transfer-Encoding"] = new[] { "chunked" };
            responseHeaders[Constants.ContentLength] = new[] { length.ToString(CultureInfo.InvariantCulture) };
            responseHeaders[Constants.ContentType] = new[] { GetContentType(file) };

            return new Tuple<long, long?>(0, length);
        }

        private static string GetContentType(string file)
        {
            // TODO: Configurable lookup table

            string contentType = null;

            int extentionIndex = file.LastIndexOf('.');
            if (extentionIndex >= 0)
            {
                string extention = file.Substring(extentionIndex);

                // Ask the registry:
                Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(extention);
                if (key != null)
                {
                    contentType = key.GetValue("Content Type") as string;
                }
            }

            return contentType ?? DefaultContentType;
        }

        private static CancellationToken GetCancellationToken(IDictionary<string, object> environment)
        {
            return (CancellationToken)environment[Constants.CallCancelledKey];
        }
    }
}
