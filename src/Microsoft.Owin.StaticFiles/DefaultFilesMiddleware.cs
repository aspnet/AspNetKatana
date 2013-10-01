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
            if (Helpers.IsGetOrHeadMethod(context.Request.Method)
                && Helpers.PathEndsInSlash(context.Request.Path) // The DirectoryBrowser will redirect for missing slashes.
                && Helpers.TryMatchPath(context, _matchUrl, forDirectory: true, subpath: out subpath)
                && subpath.HasValue && subpath.Value.Length > 0)
            {
                // Check if any of our default files exist.
                for (int matchIndex = 0; matchIndex < _options.DefaultFileNames.Count; matchIndex++)
                {
                    string defaultFile = _options.DefaultFileNames[matchIndex];
                    IFileInfo file;
                    if (_options.FileSystem.TryGetFileInfo(subpath + defaultFile, out file))
                    {
                        // Match found, re-write the url. A later middleware will actually serve the file.
                        context.Request.Path = new PathString(context.Request.Path.Value + defaultFile);
                        break;
                    }
                }
            }

            return Next.Invoke(context);
        }
    }
}
