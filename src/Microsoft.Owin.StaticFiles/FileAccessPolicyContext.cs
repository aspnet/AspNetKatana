// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.Owin.FileSystems;

namespace Microsoft.Owin.StaticFiles
{
    /// <summary>
    /// Used with IFileAccessPolicy.CheckPolicy to determine how to handle a requested file.
    /// Access is allowed by default.
    /// </summary>
    public class FileAccessPolicyContext
    {
        /// <summary>
        /// Create a new policy context.
        /// </summary>
        /// <param name="owinContext"></param>
        /// <param name="file"></param>
        public FileAccessPolicyContext(IOwinContext owinContext, IFileInfo file)
        {
            if (owinContext == null)
            {
                throw new ArgumentNullException("owinContext");
            }
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }
            OwinContext = owinContext;
            File = file;
            IsAllowed = true;
        }

        /// <summary>
        /// The Owin request and response context.
        /// </summary>
        public IOwinContext OwinContext { get; private set; }

        /// <summary>
        /// The requested file.
        /// </summary>
        public IFileInfo File { get; private set; }

        /// <summary>
        /// Indicates if the requested file be served.
        /// </summary>
        public bool IsAllowed { get; private set; }

        /// <summary>
        /// Indicates if the request should be rejected.
        /// </summary>
        public bool IsRejected { get; private set; }

        /// <summary>
        /// Indicates if the request should be passed through to the next middleware.
        /// </summary>
        public bool IsPassThrough { get; private set; }

        /// <summary>
        /// Serve the requested file.
        /// </summary>
        public void Allow()
        {
            IsAllowed = true;
            IsRejected = false;
            IsPassThrough = false;
        }

        /// <summary>
        /// Reject the requested file with the given status code.
        /// </summary>
        /// <param name="statusCode"></param>
        public void Reject(int statusCode)
        {
            OwinContext.Response.StatusCode = statusCode;
            IsAllowed = false;
            IsRejected = true;
            IsPassThrough = false;
        }

        /// <summary>
        /// Pass the request through to the next middleware.
        /// </summary>
        public void PassThrough()
        {
            IsAllowed = false;
            IsRejected = false;
            IsPassThrough = true;
        }
    }
}
