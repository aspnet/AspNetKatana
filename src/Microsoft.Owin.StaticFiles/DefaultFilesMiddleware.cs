// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles.Filters;

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

            IEnumerable<IFileInfo> dirContents;
            PathString subpath;
            if (Helpers.IsGetOrHeadMethod(context.Request.Method)
                && Helpers.TryMatchPath(context, _matchUrl, forDirectory: true, subpath: out subpath)
                && ApplyFilter(context, subpath)
                && _options.FileSystem.TryGetDirectoryContents(subpath.Value, out dirContents))
            {
                // Check if any of our default files exist.
                for (int matchIndex = 0; matchIndex < _options.DefaultFileNames.Count; matchIndex++)
                {
                    string defaultFile = _options.DefaultFileNames[matchIndex];
                    IFileInfo file;
                    // TryMatchPath will make sure subpath always ends with a "/" by adding it if needed.
                    if (_options.FileSystem.TryGetFileInfo(subpath + defaultFile, out file))
                    {
                        // If the path matches a directory but does not end in a slash, redirect to add the slash.
                        // This prevents relative links from breaking.
                        if (!Helpers.PathEndsInSlash(context.Request.Path))
                        {
                            context.Response.StatusCode = 301;
                            context.Response.Headers[Constants.Location] = context.Request.PathBase + context.Request.Path + "/";
                            return Constants.CompletedTask;
                        }

                        // Match found, re-write the url. A later middleware will actually serve the file.
                        context.Request.Path = new PathString(context.Request.Path.Value + defaultFile);
                        break;
                    }
                }
            }

            return Next.Invoke(context);
        }

        private bool ApplyFilter(IOwinContext context, PathString subpath)
        {
            if (_options.Filter == null)
            {
                return true;
            }
            RequestFilterContext filterContext = new RequestFilterContext(context, subpath);
            _options.Filter.ApplyFilter(filterContext);
            return filterContext.IsAllowed;
        }
    }
}
