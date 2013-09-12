// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Owin.FileSystems;

namespace Microsoft.Owin.StaticFiles
{
    /// <summary>
    /// This examines a directory path and determines if there is a default file present.
    /// If so the file name is appended to the path and execution continues.
    /// Note we don't just serve the file because it may require interpretation.
    /// </summary>
    public class DefaultFilesMiddleware : OwinMiddleware
    {
        private readonly DefaultFilesOptions _options;
        private readonly PathString _matchUrl;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="next"></param>
        /// <param name="options"></param>
        public DefaultFilesMiddleware(OwinMiddleware next, DefaultFilesOptions options)
            : base(next)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            _options = options;
            _matchUrl = options.RequestPath;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task Invoke(IOwinContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            PathString subpath;
            IEnumerable<IFileInfo> contents;
            string defaultFile;
            if (Helpers.IsGetOrHeadMethod(context.Request.Method)
                && Helpers.PathEndsInSlash(context.Request.Path) // The DirectoryBrowser will redirect for missing slashes.
                && Helpers.TryMatchPath(context, _matchUrl, forDirectory: true, subpath: out subpath)
                && TryGetDirectoryInfo(subpath.Value, out contents)
                && TryGetDefaultFile(contents, out defaultFile))
            {
                context.Request.Path = new PathString(context.Request.Path.Value + defaultFile);
            }

            return Next.Invoke(context);
        }

        private bool TryGetDirectoryInfo(string subpath, out IEnumerable<IFileInfo> contents)
        {
            return _options.FileSystem.TryGetDirectoryContents(subpath, out contents);
        }

        private bool TryGetDefaultFile(IEnumerable<IFileInfo> contents, out string defaultFile)
        {
            // DefaultFileNames are prioritized so we have to search in this order.
            IList<IFileInfo> files = contents.Where(file => !file.IsDirectory).ToList();
            for (int matchIndex = 0; matchIndex < _options.DefaultFileNames.Count; matchIndex++)
            {
                string matchFile = _options.DefaultFileNames[matchIndex];

                for (int fileIndex = 0; fileIndex < files.Count; fileIndex++)
                {
                    if (files[fileIndex].Name.Equals(matchFile, StringComparison.OrdinalIgnoreCase))
                    {
                        defaultFile = matchFile;
                        return true;
                    }
                }
            }

            defaultFile = null;
            return false;
        }
    }
}
