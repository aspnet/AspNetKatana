// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin.FileSystems;

namespace Microsoft.Owin.StaticFiles
{
    /// <summary>
    /// Enables directory browsing
    /// </summary>
    public class DirectoryBrowserMiddleware : OwinMiddleware
    {
        private readonly DirectoryBrowserOptions _options;
        private readonly PathString _matchUrl;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="next"></param>
        /// <param name="options"></param>
        public DirectoryBrowserMiddleware(OwinMiddleware next, DirectoryBrowserOptions options)
            : base(next)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            if (options.Formatter == null)
            {
                throw new ArgumentException(Resources.Args_NoFormatter);
            }
            if (options.FileSystem == null)
            {
                options.FileSystem = new PhysicalFileSystem("." + options.RequestPath.Value);
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

            // Check if the URL matches any expected paths
            PathString subpath;
            IEnumerable<IFileInfo> contents;
            if (Helpers.IsGetOrHeadMethod(context.Request.Method)
                && Helpers.TryMatchPath(context, _matchUrl, forDirectory: true, subpath: out subpath)
                && TryGetDirectoryInfo(subpath, out contents))
            {
                // If the path matches a directory but does not end in a slash, redirect to add the slash.
                // This prevents relative links from breaking.
                if (!Helpers.PathEndsInSlash(context.Request.Path))
                {
                    context.Response.StatusCode = 301;
                    context.Response.Headers[Constants.Location] = context.Request.PathBase + context.Request.Path + "/";
                    return Constants.CompletedTask;
                }

                return _options.Formatter.GenerateContentAsync(context, contents);
            }

            return Next.Invoke(context);
        }

        private bool TryGetDirectoryInfo(PathString subpath, out IEnumerable<IFileInfo> contents)
        {
            return _options.FileSystem.TryGetDirectoryContents(subpath.Value, out contents);
        }
    }
}
