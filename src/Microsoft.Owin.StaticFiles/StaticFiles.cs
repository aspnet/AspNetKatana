// -----------------------------------------------------------------------
// <copyright file="StaticFiles.cs" company="Katana contributors">
//   Copyright 2011-2012 Katana contributors
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

// Notes: The larger Static Files feature includes several sub modules:
// - DefaultFile: If the given path is a directory, append a default file name (if it exists on disc).
// - BrowseDirs: If the given path is for a directory, list its contents
// - StaticFiles: This module; locate an individual file and serve it.
// - SendFileFallback: Insert a SendFile delegate if none is present
namespace Microsoft.Owin.StaticFiles
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    using SendFileFunc = Func<string, long, long?, CancellationToken, Task>;

    public class StaticFiles
    {
        private IList<KeyValuePair<string, string>> _urlsAndDirs;
        private AppFunc _next;

        public StaticFiles(AppFunc next, IList<KeyValuePair<string, string>> urlsAndDirs)
        {
            _urlsAndDirs = urlsAndDirs;
            _next = next;
        }

        public Task Invoke(IDictionary<string, object> environment)
        {
            // Check if the URL matches any expected paths
            string file;
            if (TryMatchFile(environment, out file))
            {
                SendFileFunc sendFileAsync = GetSendFile(environment);
                SetHeaders(environment, file);
                return sendFileAsync(file, 0, null, GetCancellationToken(environment));
            }

            return _next(environment);
        }

        private bool TryMatchFile(IDictionary<string, object> environment, out string file)
        {
            string path = (string)environment["owin.RequestPath"];

            for (int i = 0; i < _urlsAndDirs.Count; i++)
            {
                KeyValuePair<string, string> pair = _urlsAndDirs[i];
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
            };

            file = null;
            return false;
        }

        private SendFileFunc GetSendFile(IDictionary<string, object> environment)
        {
            object obj;
            if (!environment.TryGetValue("sendfile.SendAsync", out obj)
                || !(obj is SendFileFunc))
            {
                throw new NotSupportedException("SendFileFunc not found");
            }

            return (SendFileFunc)obj;
        }

        // Set response headers:
        // Content-Length/chunked
        // Content-Type
        // TODO: Ranges
        private void SetHeaders(IDictionary<string, object> environment, string file)
        {
            var responseHeaders = (IDictionary<string, string[]>)environment["owin.ResponseHeaders"];

            FileInfo fileInfo = new FileInfo(file);
            // responseHeaders["Transfer-Encoding"] = new[] { "chunked" };
            responseHeaders["Content-Length"] = new[] { fileInfo.Length.ToString(CultureInfo.InvariantCulture) };
            responseHeaders["Content-Type"] = new[] { GetContentType(file) };
        }

        private string GetContentType(string file)
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

            return contentType ?? "application/octet-stream";
        }

        private CancellationToken GetCancellationToken(IDictionary<string, object> environment)
        {
            return (CancellationToken)environment["owin.CallCancelled"];
        }
    }
}
