// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles.Filters;

namespace Microsoft.Owin.StaticFiles.Infrastructure
{
    /// <summary>
    /// Options common to several middleware components
    /// </summary>
    public class SharedOptions
    {
        private PathString _requestPath;

        /// <summary>
        /// Defaults to all request paths and the current physical directory.
        /// </summary>
        public SharedOptions()
        {
            RequestPath = PathString.Empty;
            FileSystem = new PhysicalFileSystem(".");
            Filter = new RequestFilter();
        }

        /// <summary>
        /// The request path that maps to static resources
        /// </summary>
        public PathString RequestPath
        {
            get { return _requestPath; }
            set
            {
                if (value.HasValue && value.Value.EndsWith("/", StringComparison.Ordinal))
                {
                    throw new ArgumentException("Request path must not end in a slash");
                }
                _requestPath = value;
            }
        }

        /// <summary>
        /// The file system used to locate resources
        /// </summary>
        public IFileSystem FileSystem { get; set; }

        /// <summary>
        /// Invoked on each request to determine if the identified file or directory should be served.
        /// All files are served if this is null.
        /// </summary>
        public IRequestFilter Filter { get; set; }
    }
}
