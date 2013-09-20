// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles.DirectoryFormatters;

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
            if (options.Formatters.Count == 0)
            {
                throw new ArgumentException("No formatters provided."); // TODO:
            }
            if (options.FormatSelector == null)
            {
                throw new ArgumentException("No format selector provided."); // TODO:
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
                if (!Helpers.PathEndsInSlash(context.Request.Path))
                {
                    context.Response.StatusCode = 301;
                    context.Response.Headers[Constants.Location] = context.Request.PathBase + context.Request.Path + "/";
                    return Constants.CompletedTask;
                }

                IDirectoryInfoFormatter formatter;
                if (!TryNegotiateContent(context, out formatter))
                {
                    // 406: Not Acceptable, we couldn't generate the requested content-type.
                    context.Response.StatusCode = 406;
                    return Constants.CompletedTask;
                }

                StringBuilder body = GenerateContent(context, contents, formatter);
                context.Response.ContentType = formatter.ContentType;
                context.Response.ContentLength = body.Length;

                if (Helpers.IsGetMethod(context.Request.Method))
                {
                    // TODO: Encoding?
                    return context.Response.WriteAsync(body.ToString());
                }
                else
                {
                    // HEAD, no response body
                    return Constants.CompletedTask;
                }
            }

            return Next.Invoke(context);
        }

        private bool TryGetDirectoryInfo(PathString subpath, out IEnumerable<IFileInfo> contents)
        {
            return _options.FileSystem.TryGetDirectoryContents(subpath.Value, out contents);
        }

        // Detect the requested content-type
        private bool TryNegotiateContent(IOwinContext context, out IDirectoryInfoFormatter formatter)
        {
            return _options.FormatSelector.TryDetermineFormatter(context, _options.Formatters, out formatter);
        }

        // Generate the list of files and directories according to that type
        private static StringBuilder GenerateContent(IOwinContext context, IEnumerable<IFileInfo> contents, IDirectoryInfoFormatter formatter)
        {
            PathString requestPath = context.Request.PathBase + context.Request.Path;

            StringBuilder body = formatter.GenerateContent(requestPath, contents);

            return body;
        }
    }
}
