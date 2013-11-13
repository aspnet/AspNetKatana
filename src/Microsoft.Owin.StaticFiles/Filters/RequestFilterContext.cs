// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.Owin.FileSystems;

namespace Microsoft.Owin.StaticFiles.Filters
{
    /// <summary>
    /// Used with IFileFilter.ApplyFilter to determine how to handle a requested path.
    /// Access is allowed by default.
    /// </summary>
    public class RequestFilterContext
    {
        /// <summary>
        /// Create a new filter context.
        /// </summary>
        /// <param name="owinContext"></param>
        /// <param name="subpath"></param>
        public RequestFilterContext(IOwinContext owinContext, PathString subpath)
        {
            if (owinContext == null)
            {
                throw new ArgumentNullException("owinContext");
            }
            OwinContext = owinContext;
            Subpath = subpath;
            IsAllowed = true;
        }

        /// <summary>
        /// The OWIN request and response context.
        /// </summary>
        public IOwinContext OwinContext { get; private set; }

        /// <summary>
        /// The sub-path to the requested resource.
        /// </summary>
        public PathString Subpath { get; private set; }

        /// <summary>
        /// Indicates if the requested resource will be served.
        /// </summary>
        public bool IsAllowed { get; private set; }

        /// <summary>
        /// Indicates if the request should be passed through to the next middleware.
        /// </summary>
        public bool IsPassThrough { get; private set; }

        /// <summary>
        /// Specify that the requested resource should be served.
        /// </summary>
        public void Allow()
        {
            IsAllowed = true;
            IsPassThrough = false;
        }

        /// <summary>
        /// Specify that the request should be passed through to the next middleware.
        /// </summary>
        public void PassThrough()
        {
            IsAllowed = false;
            IsPassThrough = true;
        }
    }
}
